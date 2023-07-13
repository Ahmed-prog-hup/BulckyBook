using Bulcky.DataAccess.Data;
using Bulcky.Models;
using Bulcky.Utility;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bulcky.DataAccess.DbInitializer
{
    public class DbInitializer : IDbInitializer
    {
        private readonly UserManager<IdentityUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly ApplicationDbContext _dp;

        public DbInitializer(UserManager<IdentityUser> userManager, RoleManager<IdentityRole> roleManager,
            ApplicationDbContext dp)
        {
            _roleManager = roleManager;
            _userManager = userManager;
            _dp = dp;
        }
        public void Initailize()
        {
            // migration if there not applied

            try
            {
                if (_dp.Database.GetPendingMigrations().Count() > 0)
                {
                    _dp.Database.Migrate();
                }
            }


            catch (Exception ex) { }

            // Create Role if not Created

            if (! _roleManager.RoleExistsAsync(SD.Role_Customer).GetAwaiter().GetResult())
            {
                _roleManager.CreateAsync(new IdentityRole(SD.Role_Customer)).GetAwaiter().GetResult();
                _roleManager.CreateAsync(new IdentityRole(SD.Role_Employee)).GetAwaiter().GetResult();
                _roleManager.CreateAsync(new IdentityRole(SD.Role_Admin)).GetAwaiter().GetResult();
                _roleManager.CreateAsync(new IdentityRole(SD.Role_Company)).GetAwaiter().GetResult();

                // if not created , create admin Role

                _userManager.CreateAsync(new ApplicationUser
                {
                    UserName = "admin@gmail.com",
                    Name = "Ahmed",
                    Email = "admin@gmail.com",
                    PhoneNumber = "01128204499",
                    StreetAddress = "121 cairo",
                    State = "ITI",
                    PostalCode = "21452",
                    City = "Cairo"

                }, "Admin@123").GetAwaiter().GetResult();

                ApplicationUser user = _dp.ApplicationUsers.FirstOrDefault(u => u.Email == "admin@gmail.com");
                _userManager.AddToRoleAsync(user, SD.Role_Admin).GetAwaiter().GetResult();
            }

            return;

        }
    }
}
