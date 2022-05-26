using HotChocolate.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using SolakaDatabase.Models;
using System.Security.Claims;


namespace UserService.GraphQL
{
    public class Query
    {
        [Authorize(Roles = new[] { "AdminApp" })] // dapat diakses kalau sudah login
        public IQueryable<UserData> GetUsers([Service] SolakaDbContext context) =>
            context.Users.Select(p => new UserData()
            {
                Id = p.Id,
                Username = p.Username
            });

        [Authorize(Roles = new[] { "AdminApp" })]
        public IQueryable<RestoData> GetRestaurant([Service] SolakaDbContext context) =>
          context.Restaurants.Select(p => new RestoData()
          {
              Id = p.Id,
              NameResto = p.NameResto,
              Location = p.Location
          });

        [Authorize(Roles = new[] { "ManagerApp" })]
        public IQueryable<User> GetCustomer([Service] SolakaDbContext context)
        {
            var roleCust = context.Roles.Where(k => k.Name == "Customer").FirstOrDefault();
            var kurirs = context.Users.Where(k => k.Customers.Any(o => o.RoleId == roleCust.Id));
            return kurirs.AsQueryable();
        }
        
    }
}
