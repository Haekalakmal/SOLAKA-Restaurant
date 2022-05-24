using HotChocolate.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using SolakaDatabase.Models;
using System.Security.Claims;

namespace OrderService.GraphQL
{
    public class Query
    {
        [Authorize(Roles = new[] { "Customer" })]
        public IQueryable<Order> GetOrdersByCustomer([Service] SolakaDbContext context, ClaimsPrincipal claimsPrincipal)
        {
            var userName = claimsPrincipal.Identity.Name;
            var user = context.Users.Where(o => o.Username == userName).FirstOrDefault();
            var customer = context.Customers.Where(c => c.UserId == user.Id).FirstOrDefault();
            if (customer != null)
            {
                var orders = context.Orders.Include(o=>o.OrderDetails).Where(o => o.CustomerId == customer.Id);
                return orders.AsQueryable();
            }
            return new List<Order>().AsQueryable();
        }

        [Authorize(Roles = new[] { "ManagerApp" })]
        public IQueryable<Order> GetManagerOrders([Service] SolakaDbContext context) =>
          context.Orders.Include(o=>o.OrderDetails);

        [Authorize(Roles = new[] { "ManagerResto" })]
        public IQueryable<Order> GetManagerRestoOrders([Service] SolakaDbContext context, ClaimsPrincipal claimsPrincipal)
        {
            var userName = claimsPrincipal.Identity.Name;
            var user = context.Users.Where(o => o.Username == userName).FirstOrDefault();
            //var resto = context.Restaurants.FirstOrDefault();
            var managerResto = context.EmployeeRestos.Where(e => e.UserId == user.Id).FirstOrDefault();
            if (managerResto != null)
            {
                var orders = context.Orders.Include(o => o.OrderDetails).Where(o => o.RestoId == managerResto.RestoId);
                return orders.AsQueryable();
            }
            return new List<Order>().AsQueryable();
        }
    }
}
