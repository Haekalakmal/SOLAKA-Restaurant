using Microsoft.Extensions.Options;
using System.IdentityModel.Tokens.Jwt;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using HotChocolate.AspNetCore.Authorization;
using System.Security.Claims;
using Microsoft.EntityFrameworkCore;
using SolakaDatabase.Models;

namespace UserService.GraphQL
{
    public class Mutation
    {

        public async Task<UserData> RegisterUserAsync(
            RegisterUser input,
            [Service] SolakaDbContext context)
        {
            var user = context.Users.Where(u => u.Username == input.UserName).FirstOrDefault();
            if (user != null)
            {
                return await Task.FromResult(new UserData());
            }
            var newUser = new User
            {
                Username = input.UserName,
                Password = BCrypt.Net.BCrypt.HashPassword(input.Password) // encrypt password
            };

            // EF
            var ret = context.Users.Add(newUser);
            await context.SaveChangesAsync();

            return await Task.FromResult(new UserData
            {
                Id = newUser.Id,
                Username = newUser.Username,
            });
        }
        public async Task<UserToken> LoginAsync(
          LoginUser input,
          [Service] IOptions<TokenSettings> tokenSettings, // setting token
          [Service] SolakaDbContext context) // EF
        {
            var user = context.Users.Where(u => u.Username == input.Username).FirstOrDefault();
            if (user == null)
            {
                return await Task.FromResult(new UserToken(null, null, "Username or password was invalid"));
            }
            bool valid = BCrypt.Net.BCrypt.Verify(input.Password, user.Password);
            if (valid)
            {
                // generate jwt token
                var securitykey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(tokenSettings.Value.Key));
                var credentials = new SigningCredentials(securitykey, SecurityAlgorithms.HmacSha256);

                // jwt payload
                var claims = new List<Claim>();
                claims.Add(new Claim(ClaimTypes.Name, user.Username));

               /* var userCustomers = context.Customers.Where(b => b.UserId == user.Id).ToList();
                foreach (var userBuyer in userCustomers)
                {
                    var role = context.Roles.Where(o => o.Id == userBuyer.Id).FirstOrDefault();
                    if (role != null)
                    {
                        claims.Add(new Claim(ClaimTypes.Role, role.Name));
                    }
                }*/

                var userEmployeeApps = context.EmployeeApps.Where(c => c.UserId == user.Id).ToList();
                foreach (var userEmployeeApp in userEmployeeApps)
                {
                    var role = context.Roles.Where(o => o.Id == userEmployeeApp.Id).FirstOrDefault();
                    if (role != null)
                    {
                        claims.Add(new Claim(ClaimTypes.Role, role.Name));
                    }
                }

                var userEmployeeRestos = context.EmployeeRestos.Where(c => c.UserId == user.Id).ToList();
                foreach (var userEmployeeResto in userEmployeeRestos)
                {
                    var role = context.Roles.Where(o => o.Id == userEmployeeResto.RoleId).FirstOrDefault();
                    if (role != null)
                    {
                        claims.Add(new Claim(ClaimTypes.Role, role.Name));
                    }
                }

                var expired = DateTime.Now.AddHours(3);
                var jwtToken = new JwtSecurityToken(
                    issuer: tokenSettings.Value.Issuer,
                    audience: tokenSettings.Value.Audience,
                    expires: expired,
                    claims: claims, // jwt payload
                    signingCredentials: credentials // signature
                );

                return await Task.FromResult(
                    new UserToken(new JwtSecurityTokenHandler().WriteToken(jwtToken),
                    expired.ToString(), null));
                //return new JwtSecurityTokenHandler().WriteToken(jwtToken);
            }

            return await Task.FromResult(new UserToken(null, null, Message: "Username or password was invalid"));
        }
        [Authorize(Roles = new[] { "ADMIN" })]
        public async Task<User> UpdateUserAsync(
            UserData input,
            [Service] SolakaDbContext context)
        {
            var user = context.Users.Where(o => o.Username == input.Username).FirstOrDefault();
            if (user != null)
            {
                user.Username = input.Username;

                context.Users.Update(user);
                await context.SaveChangesAsync();
            }


            return await Task.FromResult(user);
        }
        [Authorize(Roles = new[] { "ADMIN" })]
        public async Task<User> DeleteUserByIdAsync(
            int id,
            [Service] SolakaDbContext context)
        {
            var user = context.Users.Where(o => o.Id == id).FirstOrDefault();
            if (user != null)
            {
                context.Users.Remove(user);
                await context.SaveChangesAsync();
            }


            return await Task.FromResult(user);
        }
        [Authorize]
        public async Task<User> ChangePasswordAsync(
            ChangePassword input,
            [Service] SolakaDbContext context)
        {
            var user = context.Users.Where(o => o.Id == input.Id).FirstOrDefault();
            if (user != null)
            {
                user.Password = BCrypt.Net.BCrypt.HashPassword(input.Password);

                context.Users.Update(user);
                await context.SaveChangesAsync();
            }

            return await Task.FromResult(user);
        }

        [Authorize(Roles = new[] { "ADMIN" })]
       /* public async Task<Customer> AddUserToCustomerAsync(
           AddUserToCustomerInput input,
           [Service] SolakaDbContext context)
        {
            var user = context.Users.Where(u => u.UserId == input.Id).FirstOrDefault();
            if (user == null)
            {
                return await Task.FromResult(new Customer());
            }
            // EF
            var customer = new Customer
            {
                UserId = input.UserId,
                RoleId = input.RoleId,
                Name = input.Name,
                Phone = input.Phone

            };

            var ret = context.Customers.Add(customer);
            await context.SaveChangesAsync();

            return ret.Entity;
        }*/

        [Authorize(Roles = new[] { "ADMIN", "MANAGER" })]
        public async Task<EmployeeResto> AddUserToEmployeeRestoAsync(
          AddUserToEmployeeResto input,
          [Service] SolakaDbContext context)
        {
            var user = context.Users.Where(u => u.Id == input.UserId).FirstOrDefault();
            if (user == null)
            {
                return await Task.FromResult(new EmployeeResto());
            }
            // EF
            var employeeResto = new EmployeeResto
            {
                UserId = input.UserId,
                RoleId = input.RoleId,
                RestoId = input.RestoId,
                Fullname = input.Fullname,
                Email = input.Email
            };

            var ret = context.EmployeeRestos.Add(employeeResto);
            await context.SaveChangesAsync();
            return ret.Entity;
        }

        [Authorize(Roles = new[] { "ADMIN" })]
        public async Task<EmployeeApp> AddUserToEmployeeAppAsync(
         AddUserToEmployeeApp input,
         [Service] SolakaDbContext context)
        {
            var user = context.Users.Where(u => u.Id == input.UserId).FirstOrDefault();
            if (user == null)
            {
                return await Task.FromResult(new EmployeeApp());
            }
            // EF
            var employeeApp = new EmployeeApp
            {
                UserId = input.UserId,
                RoleId = input.RoleId,
                Fullname=input.Fullname,
                Email=input.Email

            };

            var ret = context.EmployeeApps.Add(employeeApp);
            await context.SaveChangesAsync();

            return ret.Entity;
        }

    }
}
