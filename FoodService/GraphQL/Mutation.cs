using HotChocolate.AspNetCore.Authorization;
using SolakaDatabase.Models;
using System.Security.Claims;

namespace FoodService.GraphQL
{
    public class Mutation
    {
        [Authorize(Roles = new[] { "ManagerResto" })]
        public async Task<ProductData> AddFoodAsync(
           FoodInput input,
           [Service] SolakaDbContext context, ClaimsPrincipal claimsPrincipal)
        {

            var userName = claimsPrincipal.Identity.Name;
            var user = context.Users.Where(u => u.Username == userName).FirstOrDefault();
            var resto = context.Restaurants.FirstOrDefault();
            var manager = context.EmployeeRestos.Where(c => c.UserId == user.Id && c.RestoId == resto.Id).FirstOrDefault();
            //var food = context.Products.Where(o => o.Id == input.Id).FirstOrDefault();
            if (manager == null)
                return new ProductData
                {
                    TransactionDate = DateTime.Now.ToString(),
                    Message = "Manager Resto tidak punya akses!"
                };
            var newfood = new Product
            {
                Name = input.Name,
                Stock = input.Stock,
                Price = input.Price,
                CategoryId = input.CategoryId,
                RestoId = input.RestoId
            };
            context.Products.Add(newfood);
            await context.SaveChangesAsync();

            return new ProductData
            {
                TransactionDate = DateTime.Now.ToString(),
                Message = "Done Update Order!"
            };
        }

            [Authorize(Roles = new[] { "ManagerResto" })]
        public async Task<Product> UpdateFoodAsync(
           FoodUpdate input,
           [Service] SolakaDbContext context)
        {
            var food = context.Products.Where(o => o.Id == input.Id).FirstOrDefault();
            if (food != null)
            {
                food.Name = input.Name;
                food.Stock = input.Stock;
                food.Price = input.Price;
                food.CategoryId = input.CategoryId;
                food.RestoId = input.RestoId;

                context.Products.Update(food);
                await context.SaveChangesAsync();
            }


            return await Task.FromResult(food);
        }

        [Authorize(Roles = new[] { "ManagerResto" })]
        public async Task<Product> DeleteFoodByIdAsync(
           int id,
           [Service] SolakaDbContext context)
        {
            var food = context.Products.Where(o => o.Id == id).FirstOrDefault();
            if (food != null)
            {
                context.Products.Remove(food);
                await context.SaveChangesAsync();
            }


            return await Task.FromResult(food);
        }
    }
}
