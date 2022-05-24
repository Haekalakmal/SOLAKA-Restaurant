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
        //LOGIN
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

                var userEmployeeApps = context.EmployeeApps.Where(c => c.UserId == user.Id).ToList();
                foreach (var userEmployeeApp in userEmployeeApps)
                {
                    var role = context.Roles.Where(o => o.Id == userEmployeeApp.RoleId).FirstOrDefault();
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

                var userCustomers = context.Customers.Where(c => c.UserId == user.Id).ToList();
                foreach (var customers in userCustomers)
                {
                    var role = context.Roles.Where(o => o.Id == customers.RoleId).FirstOrDefault();
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

            }

            return await Task.FromResult(new UserToken(null, null, Message: "Username or password was invalid"));
        }

        //CHANGE PASSWORD
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

        //REGISTER
        [Authorize(Roles = new[] { "AdminApp" })]
        public async Task<UserData> RegisterOperatorRestoAsync(
           RegisteOperatorResto input,
           [Service] SolakaDbContext context)
        {
            var role = context.Roles.Where(o => o.Id == 2).FirstOrDefault();
            var user = context.Users.Where(o => o.Id == input.Id).FirstOrDefault();
            if (user != null)
            {
                return await Task.FromResult(new UserData());
            }
            var newUser = new User
            {
                Username = input.UserName,
                Password = BCrypt.Net.BCrypt.HashPassword(input.Password) // encrypt password
            };
            var ret = context.Users.Add(newUser);
            var employeeResto = new EmployeeResto
            {
                RoleId = role.Id,
                UserId = newUser.Id,
                RestoId = input.RestoId,
                Fullname = input.Fullname,
                Email = input.Email,
                
            };
            newUser.EmployeeRestos.Add(employeeResto);
           
            // EF
            await context.SaveChangesAsync();

            return await Task.FromResult(new UserData
            {
                Id = newUser.Id,
                Username = newUser.Username,

            });
        }

        [Authorize(Roles = new[] { "AdminApp" })]
        public async Task<UserData> RegisterManagerRestoAsync(
           RegisteOperatorResto input,
           [Service] SolakaDbContext context)
        {
            var role = context.Roles.Where(o => o.Id == 4).FirstOrDefault();
            var user = context.Users.Where(o => o.Id == input.Id).FirstOrDefault();
            if (user != null)
            {
                return await Task.FromResult(new UserData());
            }
            var newUser = new User
            {
                Username = input.UserName,
                Password = BCrypt.Net.BCrypt.HashPassword(input.Password) // encrypt password
            };
            var ret = context.Users.Add(newUser);
            var employeeResto = new EmployeeResto
            {
                RoleId = role.Id,
                UserId = newUser.Id,
                RestoId = input.RestoId,
                Fullname = input.Fullname,
                Email = input.Email,
            };
            newUser.EmployeeRestos.Add(employeeResto);
            // EF
            //var ret = context.Users.Add(newUser);
            await context.SaveChangesAsync();

            return await Task.FromResult(new UserData
            {
                Id = newUser.Id,
                Username = newUser.Username,
            });
        }

        [Authorize(Roles = new[] { "AdminApp" })]
        public async Task<UserData> RegisterManagerAppAsync(
           RegisterApp input,
           [Service] SolakaDbContext context)
        {
            var role = context.Roles.Where(o => o.Id == 3).FirstOrDefault();
            var user = context.Users.Where(o => o.Id == input.Id).FirstOrDefault();
            if (user != null)
            {
                return await Task.FromResult(new UserData());
            }
            var newUser = new User
            {
                Username = input.UserName,
                Password = BCrypt.Net.BCrypt.HashPassword(input.Password) // encrypt password
            };
            var ret = context.Users.Add(newUser);
            var employeeApp = new EmployeeApp
            {
                RoleId = role.Id,
                UserId = newUser.Id,
                Fullname = input.Fullname,
                Email = input.Email,
                Created = DateTime.Now,
                
            };
            newUser.EmployeeApps.Add(employeeApp);
            // EF
            
            await context.SaveChangesAsync();

            return await Task.FromResult(new UserData
            {
                Id = newUser.Id,
                Username = newUser.Username,

            });
        }

        public async Task<UserData> RegisterCustomerAsync(
           RegisterCustomer input,
           [Service] SolakaDbContext context)
        {
            var role = context.Roles.Where(o => o.Id == 5).FirstOrDefault();
            var user = context.Users.Where(o => o.Id == input.Id).FirstOrDefault();
            if (user != null)
            {
                return await Task.FromResult(new UserData());
            }
            var newUser = new User
            {
                Username = input.UserName,
                Password = BCrypt.Net.BCrypt.HashPassword(input.Password) // encrypt password
            };
            var ret = context.Users.Add(newUser);
            await context.SaveChangesAsync();
           
            var customer = new Customer
            {
                RoleId = role.Id,
                UserId = newUser.Id,
                Name = input.Name,
                Phone = input.Phone
            };
            newUser.Customers.Add(customer);
            // EF
            await context.SaveChangesAsync();
            return await Task.FromResult(new UserData
            {
                Id = newUser.Id,
                Username = newUser.Username,

            });
        }

        [Authorize(Roles = new[] { "ManagerResto" })]
        public async Task<Customer> UpdateCustomerAsync(
            RegisterCustomer input,
            [Service] SolakaDbContext context)
        {
            var cust = context.Customers.Where(o => o.Id == input.Id).FirstOrDefault();
            if (cust != null)
            {

                cust.Name = input.Name;

                context.Customers.Update(cust);
                await context.SaveChangesAsync();
            }

            return await Task.FromResult(cust);
        }

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

        //UPDATE USER
        [Authorize(Roles = new[] { "ManagerApp" })]
        public async Task<User> UpdateUserAsync(
            UserData input,
            [Service] SolakaDbContext context)
        {
            var user = context.Users.Where(o => o.Id == input.Id).FirstOrDefault();
            if (user != null)
            {

                user.Username = input.Username;

                context.Users.Update(user);
                await context.SaveChangesAsync();
            }

            return await Task.FromResult(user);
        }

        //DELETE USER
        [Authorize(Roles = new[] { "ManagerApp" })]
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

        //Manage RESTO
        [Authorize(Roles = new[] { "ManagerApp" })]
        public async Task<RestoData> AddRestoAsync(
            AddResto input,
            [Service] SolakaDbContext context)
        {
            var resto = context.Restaurants.Where(u => u.NameResto == input.NameResto).FirstOrDefault();
            if (resto != null)
            {
                return await Task.FromResult(new RestoData());
            }
            var newResto = new Restaurant
            {
                NameResto = input.NameResto,
                Location = input.Location
            };
            // EF
            var ret = context.Restaurants.Add(newResto);
            await context.SaveChangesAsync();

            return await Task.FromResult(new RestoData
            {
                Id = newResto.Id,
                NameResto = newResto.NameResto,
                Location = newResto.Location
            });
        }
        [Authorize(Roles = new[] { "ManagerResto" })]
        public async Task<Restaurant> UpdateRestoAsync(
            AddResto input,
            [Service] SolakaDbContext context)
        {
            var resto = context.Restaurants.Where(o => o.Id == input.Id).FirstOrDefault();
            if (resto != null)
            {

                resto.NameResto = input.NameResto;
                resto.Location = input.Location;

                context.Restaurants.Update(resto);
                await context.SaveChangesAsync();
            }
            return await Task.FromResult(resto);
        }
        [Authorize(Roles = new[] { "ManagerResto" })]
        public async Task<Restaurant> DeleteRestoByIdAsync(
            int id,
            [Service] SolakaDbContext context)
        {
            var resto = context.Restaurants.Where(o => o.Id == id).FirstOrDefault();
            if (resto != null)
            {
                context.Restaurants.Remove(resto);
                await context.SaveChangesAsync();
            }


            return await Task.FromResult(resto);
        }
    }
}
