using Bookstore.DataAccess.Data;
using Bookstore.Models.Models;
using Bookstore.Utility;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace Bookstore.DataAccess.DbInitializer
{
    /// <summary>
    /// Create roles and first admin user if they don't exist
    /// </summary>
    public class DbInitializer : IDbInitializer
    {

        private readonly UserManager<IdentityUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly ApplicationDbContext _db;

        public DbInitializer(
            UserManager<IdentityUser> userManager,
            RoleManager<IdentityRole> roleManager,
            ApplicationDbContext db
            )
        {
            _userManager = userManager;
            _roleManager = roleManager;
            _db = db;
        }

        public void Initialize()
        {
            //push migrations if they are not applied
            try
            {
                if (_db.Database.GetPendingMigrations().Any())
                {
                    _db.Database.Migrate();
                }
            }
            catch (Exception ex)
            {
                return;
            }


            //create roles if they are not created
            //this is the same as await
            if (!_roleManager.RoleExistsAsync(ConstantDefines.Role_Customer).GetAwaiter().GetResult())
            {
                _roleManager.CreateAsync(new IdentityRole(ConstantDefines.Role_Admin)).GetAwaiter().GetResult();
                _roleManager.CreateAsync(new IdentityRole(ConstantDefines.Role_Employee)).GetAwaiter().GetResult();
                _roleManager.CreateAsync(new IdentityRole(ConstantDefines.Role_Customer)).GetAwaiter().GetResult();
                _roleManager.CreateAsync(new IdentityRole(ConstantDefines.Role_Company)).GetAwaiter().GetResult();

                //if roles are not created, create admin user
                _userManager.CreateAsync(new ApplicationUser()
                {
                    UserName = "admin@test123.com",
                    Email = "admin@test123.com",
                    Name = "Master",
                    PhoneNumber = "111222333",
                    StreetAddress = "test",
                    State = "Empty",
                    PostalCode = "11512",
                    City = "PIT"
                }, "AdminTest1!").GetAwaiter().GetResult();

                ApplicationUser user = _db.ApplicationUsers.FirstOrDefault(u => u.Email == "admin@test123.com");
                _userManager.AddToRoleAsync(user, ConstantDefines.Role_Admin).GetAwaiter().GetResult();
            }

            return;
        }
    }
}
