using HotChocolate.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using SolakaDatabase.Models;
using System.Security.Claims;


/*namespace UserService.GraphQL
{
    public class Query
    {

        [Authorize(Roles = new[] { "ADMIN" })] // dapat diakses kalau sudah login
        public IQueryable<UserData> GetUsers([Service] FoodDeliveryContext context) =>
            context.Users.Include(o=>o.Profiles).Select(p => new UserData()
            {
                Id = p.Id,
                FullName = p.FullName,
                Email = p.Email,
                Username = p.Username
            });

        [Authorize]
        public IQueryable<Profile> GetProfilesbyToken([Service] 
        FoodDeliveryContext context, ClaimsPrincipal claimsPrincipal)
        {
            var userName = claimsPrincipal.Identity.Name;
            var user = context.Users.Where(o => o.Username == userName).FirstOrDefault();
            if (user != null)
            {
                var profiles = context.Profiles.Where(o => o.UserId == user.Id);
                return profiles.AsQueryable();
            }
            return new List<Profile>().AsQueryable();
        }

        [Authorize(Roles = new[] { "MANAGER" })]
        public IQueryable<User> GetCouriers([Service] FoodDeliveryContext context)
        {
            var roleKurir = context.Roles.Where(k => k.Name == "COURIER").FirstOrDefault();
            var kurirs = context.Users.Where(k => k.UserRoles.Any(o => o.RoleId == roleKurir.Id));
            return kurirs.AsQueryable();
        }
        [Authorize(Roles = new[] { "MANAGER" })]
        public IQueryable<Courier> GetCourierProfiles([Service] FoodDeliveryContext context) =>
            context.Couriers.Select(p => new Courier()
            {
                Id = p.Id,
                CourierName = p.CourierName,
                Phone = p.Phone,
                UserId = p.UserId,
                Availibility = p.Availibility
            });


    }
}
*/