using HotChocolate.AspNetCore.Authorization;
using SolakaDatabase.Models;

namespace FoodService.GraphQL
{
    public class Mutation
    {
        [Authorize(Roles = new[] { "ManagerResto" })]
        public async Task<ProductData> AddFoodAsync(
          FoodInput input,
          [Service] SolakaDbContext context)
        {
            var food = context.Products.Where(o => o.Id == input.Id).FirstOrDefault();
            if (food != null)
            {
                return await Task.FromResult(new ProductData());
            }
            var newfood = new Product
            {
                Name = input.Name,
                Stock = input.Stock,
                Price = input.Price,
                CategoryId = input.CategoryId,
                RestoId = input.RestoId
            };

            // EF
            var ret = context.Products.Add(newfood);
            await context.SaveChangesAsync();

            return await Task.FromResult(new ProductData
            {
                Id = newfood.Id,
                Name = newfood.Name,
                Stock = newfood.Stock,
                Price = newfood.Price,
                CategoryId = newfood.CategoryId,
                RestoId = newfood.RestoId
            });
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
