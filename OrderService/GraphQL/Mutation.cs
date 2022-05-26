using HotChocolate.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using OrderService.GraphQL;
using OrderService.Kafka;
using SolakaDatabase.Models;
using System.Security.Claims;

namespace OrderService.GraphQL
{
    public class Mutation
    {
        [Authorize(Roles = new[] { "Customer" })]
        public async Task<OrdersOutput> AddOrderAsync(
            OrdersInput input, ClaimsPrincipal claimsPrincipal,
            [Service] SolakaDbContext context,
            [Service] IOptions<KafkaSettings> settings)
        {
            using var transaction = context.Database.BeginTransaction();
            var userName = claimsPrincipal.Identity.Name;
            try
            {
                var user = context.Users.Where(u => u.Username == userName).FirstOrDefault();
                var customer = context.Customers.Include(c=>c.Orders).Where(c => c.UserId == user.Id).FirstOrDefault();
                if (customer != null)
                {
                    //cek status order 
                    int orderCustomer = customer.Orders.Where(o=>o.Status== StatusOrder.Pending).Count();
                    if (orderCustomer >= 3) return new OrdersOutput
                    {
                        TransactionDate = DateTime.Now.ToString(),
                        Message = "Tidak dapat membuat order baru, mohon selesaikan pembayaran order anda sebelumnya!"
                    };

                    double totalCost = 0.0;
                    var order = new Order
                    {
                        CustomerId = customer.Id,
                        TransactionCode =  "077" + customer.Phone,
                        RestoId = input.RestoId,
                        PaymentId = input.PaymentId,
                        Status = StatusOrder.Pending,
                        Created = DateTime.Now
                    };
                    foreach (var item in input.ListOrderDetails)
                    {
                        //cek stock
                        var product = context.Products.Where(p => p.Id == item.ProductId).FirstOrDefault();
                        //if (product.Stock <= item.Quantity) continue; 
                        if(product.Stock < item.Quantity) return new OrdersOutput
                        {
                            TransactionDate = DateTime.Now.ToString(),
                            Message = "Gagal Membuat Order, Stock Tidak Cukup!"
                        };

                        if(product.RestoId != input.RestoId) return new OrdersOutput
                        {
                            TransactionDate = DateTime.Now.ToString(),
                            Message = "Gagal Membuat Order, Produk pada resto tidak tersedia!"
                        };

                        var details = new OrderDetail
                        {
                            OrderId = order.Id,
                            ProductId = item.ProductId,
                            Quantity = item.Quantity,
                            Cost = product.Price * item.Quantity,
                        };
                        order.OrderDetails.Add(details);
                        totalCost += details.Cost;
                        product.Stock -= item.Quantity;

                    }
                    order.TotalCost = totalCost;
                    context.Orders.Add(order);
                    context.SaveChanges();
                    await transaction.CommitAsync();

                    //SendDataOrder dengan Kafka
                    SendDataOrder virtualAccount = new SendDataOrder
                    {
                        TransactionId = order.Id,
                        Virtualaccount = order.TransactionCode, //0778 + phone
                        Bills = Convert.ToString(order.TotalCost), //total cost
                        PaymentStatus = order.Status
                    };

                    var key = DateTime.Now.ToString();
                    var val = JsonConvert.SerializeObject(virtualAccount);
                    if (order.PaymentId == 1)
                    {
                        bool result = KafkaHelper.SendMessage(settings.Value, "OPO", key, val).Result;
                        if (result)
                        {
                            Console.WriteLine("Sukses Kirim ke Kafka");
                        }
                        else
                        {
                            Console.WriteLine("Gagal Kirim ke Kafka");
                        }
                    }
                    else if (order.PaymentId == 2)
                    {
                        bool result = KafkaHelper.SendMessage(settings.Value, "BANK", key, val).Result;
                        if (result)
                        {
                            Console.WriteLine("Sukses Kirim ke Kafka");
                        }
                        else
                        {
                            Console.WriteLine("Gagal Kirim ke Kafka");
                        }
                    }
                    else
                    {
                        throw new Exception("Payment Not Available");
                    }

                    return new OrdersOutput
                    {
                        TransactionDate = DateTime.Now.ToString(),
                        Message = "Berhasil Membuat Order!"
                    };
                }
                else
                {
                    throw new Exception("user was not found");
                }       
            }
            catch (Exception ex)
            {
                transaction.Rollback();
                return new OrdersOutput
                {
                    TransactionDate = DateTime.Now.ToString(),
                    Message = ex.Message
                };
            }
        }

        [Authorize(Roles = new[] { "Customer" })]
        public async Task<OrdersOutput> UpdateOrderByCustomerAsync(
            OrdersUpdate input,
            ClaimsPrincipal claimsPrincipal,
            [Service] SolakaDbContext context)
        {
            using var transaction = context.Database.BeginTransaction();

            var userName = claimsPrincipal.Identity.Name;
            try
            {
                var user = context.Users.Where(o => o.Username == userName).FirstOrDefault();

                var order = context.Orders.Where(o => o.Id == input.Id).FirstOrDefault();
                var customer = context.Customers.Include(c => c.Orders).Where(c => c.UserId == user.Id && c.Id == order.CustomerId).FirstOrDefault();

                if (customer == null) return new OrdersOutput
                {
                    TransactionDate = DateTime.Now.ToString(),
                    Message = "Customer tidak ada akses!"
                };

                if (order == null || order.Status != StatusOrder.Pending) return new OrdersOutput
                {
                    TransactionDate = DateTime.Now.ToString(),
                    Message = "Order Tidak Ditemukan atau Order telah selesai"
                };

                double totalCost = 0.0;
                order.RestoId = input.RestoId;
                order.PaymentId = input.PaymentId;

                foreach (var item in input.ListOrderDetailsUpdate)
                {
                    var orderDetail = context.OrderDetails.Where(o => o.OrderId == order.Id && o.Id == item.Id).FirstOrDefault();
                    if (orderDetail == null) return new OrdersOutput
                    {
                        TransactionDate = DateTime.Now.ToString(),
                        Message = "Order Tidak Ditemukan!"
                    };

                    var product = context.Products.Where(p => p.Id == item.ProductId).FirstOrDefault();

                    if (product.RestoId != input.RestoId) return new OrdersOutput
                    {
                        TransactionDate = DateTime.Now.ToString(),
                        Message = "Gagal Update Order, Produk pada resto tidak tersedia!"
                    };
                    //var product = context.Products.FirstOrDefault();

                    orderDetail.Id = item.Id;
                    orderDetail.ProductId = item.ProductId;
                    orderDetail.Quantity = item.Quantity;
                    orderDetail.Cost = product.Price * item.Quantity;

                    order.OrderDetails.Add(orderDetail);
                    totalCost += orderDetail.Cost;
                }

                order.TotalCost = totalCost;
                context.Orders.Update(order);

                context.SaveChanges();
                await transaction.CommitAsync();

                return new OrdersOutput
                {
                    TransactionDate = DateTime.Now.ToString(),
                    Message = "Berhasil Update Order!"
                };

            }
            catch (Exception ex)
            {
                transaction.Rollback();

                return new OrdersOutput
                {
                    TransactionDate = DateTime.Now.ToString(),
                    Message = ex.Message
                };
            }
        }

        [Authorize(Roles = new[] { "Customer" })]
        public async Task<OrdersOutput> CancleOrderByCustomerAsync(
            StatusOrderInput input, ClaimsPrincipal claimsPrincipal,
            [Service] SolakaDbContext context)
        {
            var userName = claimsPrincipal.Identity.Name;

            var user = context.Users.Where(o => o.Username == userName).FirstOrDefault();
            var order = context.Orders.Where(o => o.Id == input.Id).FirstOrDefault();
            //var order = context.Orders.FirstOrDefault();
            var customer = context.Customers.Where(o => o.UserId == user.Id && o.Id == order.CustomerId).FirstOrDefault();
            if (customer == null)
            {
                return new OrdersOutput
                {
                    TransactionDate = DateTime.Now.ToString(),
                    Message = "User tidak ada akses!"
                };
            }
            if (order != null)
            {
                order.Status = StatusOrder.Cancel;

                context.Orders.Update(order);
                await context.SaveChangesAsync();
            }
            return new OrdersOutput
            {
                TransactionDate = DateTime.Now.ToString(),
                Message = "Berhasil Membatalkan Pesanan!"
            };
        }

        //[Authorize(Roles = new[] { "ManagerResto" })]
        //public async Task<OrderDetail> UpdateStatusAsync(
        //    OrdersUpdate input,
        //    [Service] SolakaDbContext context)
        //{
        //    var orderDetail = context.OrderDetails.Where(o => o.Id == input.Id).FirstOrDefault();
        //    if (orderDetail != null)
        //    {
        //        orderDetail.Status = "SUDAH DIBAYAR";
        //        context.OrderDetails.Update(orderDetail);
        //        await context.SaveChangesAsync();
        //    }
        //    return await Task.FromResult(orderDetail);
        //}

        [Authorize(Roles = new[] { "ManagerApp" })]
        public async Task<Order> DeleteOrderByIdAsync(
            int id,
            [Service] SolakaDbContext context)
        {
            var order = context.Orders.Where(o => o.Id == id).FirstOrDefault();
            //var OrderDetail = context.OrderDetails.Where(x => x.OrderId == Order.)
            if (order != null)
            {
                context.OrderDetails.RemoveRange(context.OrderDetails.Where(x => x.OrderId == id));
                context.Orders.RemoveRange(context.Orders.Where(x => x.Id == id));

                await context.SaveChangesAsync();
            }
            return await Task.FromResult(order);
        }

        [Authorize(Roles = new[] { "ManagerResto" })]
        public async Task<OrdersOutput> UpdateOrderRestoByIdAsync(
            OrderUpdateByResto input, 
            ClaimsPrincipal claimsPrincipal,
            [Service] SolakaDbContext context)
        {
            var userName = claimsPrincipal.Identity.Name;
            var user = context.Users.Where(o => o.Username == userName).FirstOrDefault();

            var resto = context.Restaurants.FirstOrDefault();
            var managerResto = context.EmployeeRestos.Where(o => o.UserId == user.Id && o.RestoId == resto.Id).FirstOrDefault();

            var same = context.Orders.Where(o => o.RestoId == resto.Id).FirstOrDefault();
            var updateorderresto = context.Orders.Where(o => o.Id == input.Id).FirstOrDefault();

            if (managerResto == null)

                return new OrdersOutput
                {
                    TransactionDate = DateTime.Now.ToString(),
                    Message = "Manager tidak ada akses!"
                };

            if (same != null)
            {
                updateorderresto.TransactionCode = input.TransactionCode;

                context.Orders.Update(updateorderresto);
                await context.SaveChangesAsync();

                return new OrdersOutput
                {
                    TransactionDate = DateTime.Now.ToString(),
                    Message = "Done Update Order!"
                };
            }
            else
            {
                return new OrdersOutput
                {
                    TransactionDate = DateTime.Now.ToString(),
                    Message = "Gagal Update Order!"
                };
            }
        }
    }
}
