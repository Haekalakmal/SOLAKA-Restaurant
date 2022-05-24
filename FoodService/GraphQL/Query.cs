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
                if (adminRole.Value == "Customer" || adminRole.Value == "ManagerResto")
                {
                    return context.Products;
                }


            }


            return new List<Product>().AsQueryable();
        }
    }
}
