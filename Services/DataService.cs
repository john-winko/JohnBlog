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
            // TODO: check if schema is correct, drop/add and populate from a csv if not
            
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
            // Seeding more programmatically since these changes use UserManager to affect changes
            // rather than simply dbcontext (would have to load all 7 of the asp net tables)

            var config = new ConfigurationBuilder()
                .SetBasePath(AppDomain.CurrentDomain.BaseDirectory)
                .AddJsonFile("Data/Seed.json")
                .Build()
                .GetSection("Users")
                .Get<List<BlogUserSeed>>();
            
            var missingUsers = config
                .Where(blogUserSeed => !userManager.Users
                    .Any(p => p.Email == blogUserSeed.Email));
            
            foreach (var blogUserSeed in missingUsers)
            {
                // explicit conversion for usermanager
                BlogUser toAdd = blogUserSeed;
                await userManager.CreateAsync(toAdd, blogUserSeed.Password);
                foreach (var blogRole in blogUserSeed.Roles)
                {
                    await userManager.AddToRoleAsync(toAdd, blogRole.ToString());
                }
            }
        }
    }
    
    // BlogUser doesn't match 1:1 to what is needed to seed Asp.Net users
    public class BlogUserSeed : BlogUser
    {
        public List<BlogRole> Roles { get; set; } = new();
        public string? Password { get; set; }
    }
}
