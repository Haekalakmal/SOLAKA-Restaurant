using HotChocolate.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using SolakaDatabase.Models;
using System.Security.Claims;


namespace UserService.GraphQL
{
    public class Query
    {

        [Authorize(Roles = new[] { "ADMIN" })] // dapat diakses kalau sudah login
        public IQueryable<UserData> GetUsers([Service] SolakaDbContext context) =>
            context.Users.Select(p => new UserData()
            {
                Id = p.Id,
                Username = p.Username
            });

        //[Authorize]
        //public IQueryable<Profile> GetProfilesbyToken([Service]
        //SolakaDbContext context, ClaimsPrincipal claimsPrincipal)
        //{
        //    var userName = claimsPrincipal.Identity.Name;
        //    var user = context.Users.Where(o => o.Username == userName).FirstOrDefault();
        //    if (user != null)
        //    {
        //        var profiles = context.Profiles.Where(o => o.UserId == user.Id);
        //        return profiles.AsQueryable();
        //    }
        //    return new List<Profile>().AsQueryable();
        //}

        [Authorize(Roles = new[] { "MANAGER" })]
        public IQueryable<User> GetCustomer([Service] SolakaDbContext context)
        {
            var roleCust = context.Roles.Where(k => k.Name == "Customer").FirstOrDefault();
            var kurirs = context.Users.Where(k => k.Customers.Any(o => o.RoleId == roleCust.Id));
            return kurirs.AsQueryable();
        }
        //[Authorize(Roles = new[] { "MANAGER" })]
        //public IQueryable<Courier> GetCourierProfiles([Service] FoodDeliveryContext context) =>
        //    context.Couriers.Select(p => new Courier()
        //    {
        //        Id = p.Id,
        //        CourierName = p.CourierName,
        //        Phone = p.Phone,
        //        UserId = p.UserId,
        //        Availibility = p.Availibility
        //    });


    }
}
