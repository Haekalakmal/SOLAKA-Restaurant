﻿using HotChocolate.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using OrderService.GraphQL;
using SolakaDatabase.Models;
using System.Security.Claims;

namespace OrderService.GraphQL
{
    public class Mutation
    {
        [Authorize(Roles = new[] { "Customer" })]
        public async Task<OrdersOutput> AddOrderAsync(
            OrdersInput input, ClaimsPrincipal claimsPrincipal,
            [Service] SolakaDbContext context)
        {
            using var transaction = context.Database.BeginTransaction();
            var userName = claimsPrincipal.Identity.Name;
            try
            {
                var user = context.Users.Where(u => u.Username == userName).FirstOrDefault();
                var customer = context.Customers.Where(c => c.UserId == user.Id).FirstOrDefault();
                if (customer != null)
                {
                    var order = new Order
                    {
                        CustomerId = customer.Id,
                        Invoice = input.Invoice + "-" + customer.Phone,
                        RestoId = input.RestoId,
                        PaymentId = input.PaymentId,
                        Created = DateTime.Now
                    };
                    foreach (var item in input.ListOrderDetails)
                    {
                        var product = context.Products.Where(p => p.Id == item.ProductId).FirstOrDefault();
                        var details = new OrderDetail
                        {
                            OrderId = order.Id,
                            ProductId = item.ProductId,
                            Quantity = item.Quantity,
                            Cost = product.Price * item.Quantity,
                            Status = StatusOrder.WaitingForPayment
                        };
                        order.OrderDetails.Add(details);
                    }
                    context.Orders.Add(order);
                    context.SaveChanges();
                    await transaction.CommitAsync();

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
        public async Task<OrderDetail> UpdateOrderByCustomerAsync(
            OrdersUpdate input,
            [Service] SolakaDbContext context)
        {
            var orderDetail = context.OrderDetails.Where(o => o.Id == input.Id).FirstOrDefault();
            if (orderDetail != null)
            {
                orderDetail.Quantity = input.Quantity;
                context.OrderDetails.Update(orderDetail);
                await context.SaveChangesAsync();
            }
            return await Task.FromResult(orderDetail);
        }

        [Authorize(Roles = new[] { "ManagerResto" })]
        public async Task<OrderDetail> UpdateStatusAsync(
            OrdersUpdate input,
            [Service] SolakaDbContext context)
        {
            var orderDetail = context.OrderDetails.Where(o => o.Id == input.Id).FirstOrDefault();
            if (orderDetail != null)
            {
                orderDetail.Status = "SUDAH DIBAYAR";
                context.OrderDetails.Update(orderDetail);
                await context.SaveChangesAsync();
            }
            return await Task.FromResult(orderDetail);
        }

        [Authorize(Roles = new[] { "Customer" })]
        public async Task<OrderDetail> CancelOrderAsync(
       StatusOrderInput input, ClaimsPrincipal claimsPrincipal,
       [Service] SolakaDbContext context)
        {
            var userName = claimsPrincipal.Identity.Name;
            var user = context.Users.Where(o => o.Username == userName).FirstOrDefault();
            var orderDetail = context.OrderDetails.Where(o => o.Id == input.Id).FirstOrDefault();

            if (user != null)
            {
                orderDetail.Status = "CANCEL";
                context.OrderDetails.Update(orderDetail);
                await context.SaveChangesAsync();
            }
            return await Task.FromResult(orderDetail);
        }

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
        public async Task<Restaurant> UpdateRestoByIdAsync(
       RestoUpdate input, ClaimsPrincipal claimsPrincipal,
      [Service] SolakaDbContext context)
        {
            var userName = claimsPrincipal.Identity.Name;
            var user = context.Users.Where(o => o.Username == userName).FirstOrDefault();
            var resto = context.Restaurants.Include(o => o.EmployeeRestos).Where(o => o.Id == user.Id).FirstOrDefault();
            var updateresto = context.Restaurants.Where(o => o.Id == input.Id).FirstOrDefault();
                if (resto != null)
                {
                    updateresto.Location = input.Location;
                    updateresto.NameResto = input.NameResto;
                    context.Restaurants.Update(updateresto);
                    await context.SaveChangesAsync();
                }
                return await Task.FromResult(resto);
            }    
    }
}
