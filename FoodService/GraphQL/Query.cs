using HotChocolate.AspNetCore.Authorization;
using SolakaDatabase.Models;
using System.Security.Claims;

namespace FoodService.GraphQL
{
    public class Query
    {
        [Authorize] // dapat diakses kalau sudah login
        public IQueryable<Product> GetFoods([Service] SolakaDbContext context, ClaimsPrincipal claimsPrincipal)
        {
            var userName = claimsPrincipal.Identity.Name;

            // check admin role ?
            var adminRole = claimsPrincipal.Claims.Where(p => p.Type == ClaimTypes.Role).FirstOrDefault();
            var user = context.Users.Where(o => o.Username == userName).FirstOrDefault();
            if (user != null)
            {
                if (adminRole.Value == "Customer")
                {
                    return context.Products;
                }


            }


            return new List<Product>().AsQueryable();
        }

        [Authorize(Roles = new[] { "ManagerResto" })]
        public IQueryable<Product> GetFoodsByManagerResto([Service] SolakaDbContext context, ClaimsPrincipal claimsPrincipal)
        {
            var userName = claimsPrincipal.Identity.Name;
            var user = context.Users.Where(o => o.Username == userName).FirstOrDefault();
            //var resto = context.Restaurants.FirstOrDefault();
            var managerResto = context.EmployeeRestos.Where(e => e.UserId == user.Id).FirstOrDefault();

            if (managerResto != null)
            {
                var food = context.Products.Where(o => o.RestoId == managerResto.RestoId);
                return food.AsQueryable();
            }
            return new List<Product>().AsQueryable();
        }
    }
}
