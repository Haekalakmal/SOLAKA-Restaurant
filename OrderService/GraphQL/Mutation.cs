using HotChocolate.AspNetCore.Authorization;
using OrderServices.GraphQL;
using SolakaDatabase.Models;
using System.Security.Claims;

namespace OrderService.GraphQL
{
    public class Mutation
    {
        [Authorize(Roles = new[] { "ManagerAPP" })]
        public async Task<OrderData> AddOrderAsync(
            OrderData input, ClaimsPrincipal claimsPrincipal,
            [Service] SolakaDbContext context)
        {

            using var transaction = context.Database.BeginTransaction();
            var userName = claimsPrincipal.Identity.Name;

            try
            {


                var user = context.Users.Where(o => o.Username == userName).FirstOrDefault();


                if (user != null)
                {


                    var order = new Order
                    {
                        CustomerId = user.Id,
                        Invoice = Guid.NewGuid().ToString(),
                        ProductId = input.ProductId,
                        PaymentId = input.PaymentId,
                        Created = DateTime.Now


                    };



                    foreach (var item in input.Details)
                    {
                        var detial = new OrderDetail
                        {
                            OrderId = order.Id,
                            Cost = item.Cost,
                            Quantity = item.Quantity,
                            Status = "BELUM DIBAYAR"
                        };
                        order.OrderDetails.Add(detial);
                    }
                    context.Orders.Add(order);
                    context.SaveChanges();
                    await transaction.CommitAsync();

                    input.Id = order.Id;
                    input.Invoice = order.Invoice;
                    input.CustomerId = order.CustomerId;


                }
            
                else
                throw new Exception("user was not found");
        }
            catch(Exception err)
            {
                transaction.Rollback();
            }



            return input;
        }

        [Authorize(Roles = new[] { "ManagerAPP" })]
        public async Task<OrderDetail> UpdateOrderAsync(
                       OrdersUpdate input,
                       [Service] SolakaDbContext context)
        {
            var orderDetail = context.OrderDetails.Where(o => o.Id == input.Id).FirstOrDefault();
            if (orderDetail != null)
            {

              


                orderDetail.Quantity = input.Quantity;

                orderDetail.Cost = input.Cost;

                context.OrderDetails.Update(orderDetail);
                await context.SaveChangesAsync();
            }

            return await Task.FromResult(orderDetail);

        }

        [Authorize(Roles = new[] { "ManagerAPP" })]
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
    }
}
