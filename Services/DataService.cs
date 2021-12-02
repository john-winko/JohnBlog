using JohnBlog.Data;
using JohnBlog.Enums;
using JohnBlog.Models;
using Microsoft.AspNetCore.Identity;

namespace JohnBlog.Services
{
    // TODO: use a json file to populate users, example posts etc
    public class DataService
    {
        private readonly ApplicationDbContext dbContext;
        private readonly RoleManager<IdentityRole> roleManager;
        private readonly UserManager<BlogUser> userManager;

        public DataService(RoleManager<IdentityRole> roleManager, UserManager<BlogUser> userManager, ApplicationDbContext dbContext)
        {
            this.roleManager = roleManager;
            this.userManager = userManager;
            this.dbContext = dbContext;
        }

        public async Task SeedDatabaseAsync()
        {
            await dbContext.Database.EnsureCreatedAsync();
            await SeedRolesAsync();
            await SeedUsersAsync();
        }

        private async Task SeedRolesAsync()
        {
            if (dbContext.Roles.Any()) return;

            foreach (var role in Enum.GetNames(typeof(BlogRole)))
            {
                await roleManager.CreateAsync(new IdentityRole(role));
            }
        }

        private async Task SeedUsersAsync()
        {
            if (dbContext.Users.Any()) return;

            var adminUser = new BlogUser()
            {
                Email = "john.winko@gmail.com",
                UserName = "Admin-John",                
                FirstName = "John",
                LastName = "Winko",
                PhoneNumber = "(904) 703-4856",
                EmailConfirmed = true
            };
            await userManager.CreateAsync(adminUser, "asdfASDF1234!@#$");
            await userManager.AddToRoleAsync(adminUser, BlogRole.Administrator.ToString());

            var moderatorUser = new BlogUser()
            {
                Email = "eternal81@msn.com",
                UserName = "Mod-John",
                FirstName = "John",
                LastName = "Winko",
                PhoneNumber = "(904) 703-4856",
                EmailConfirmed = true
            };           
            await userManager.CreateAsync(moderatorUser, "Abc&123!");
            await userManager.AddToRoleAsync(moderatorUser, BlogRole.Moderator.ToString());
        }
    }
}
