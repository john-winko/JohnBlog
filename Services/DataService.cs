using System.Globalization;
using CsvHelper;
using CsvHelper.Configuration;
using CsvHelper.TypeConversion;
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
            
            var csv = new CsvReader(
                new StreamReader("D:/temp/AspNetUsers.csv"), CultureInfo.InvariantCulture);
            csv.Context.RegisterClassMap<AspNetUsersMap>();
            var records = csv.GetRecords<BlogUser>();
            foreach (var bRecord in records)
            {
                // TODO: Current issue where it enters one ID correctly but the other incorrectly... double check mapping isn't getting borked with a required field
                if (!dbContext.Users!.Any(p => p.Email == bRecord.Email))
                {
                    dbContext.Users!.Add(bRecord);
                }
            }

            await dbContext.SaveChangesAsync();
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

    public sealed class BlogMap : ClassMap<Blog>
    {
        // ID's are auto generated so can't have proper FK relationships when seeding
        public BlogMap()
        {
            Map(m => m.Id).Name("Id");
            Map(m => m.BlogUserId).Name("BlogUserId");
            Map(m => m.Name).Name("Name");
            Map(m => m.Description).Name("Description");
            Map(m => m.Created).Name("Created");
            Map(m => m.Updated).Name("Updated");
            Map(m => m.BlogImage).Name("BlogImage");
        }
    }
    public sealed class AspNetUsersMap : ClassMap<BlogUser>
    {
        public AspNetUsersMap()
        {
            AutoMap(CultureInfo.InvariantCulture);
            
            Map(m => m.NormalizedEmail).Ignore();
            Map(m => m.NormalizedUserName).Ignore();

            Map(m => m.EmailConfirmed).Name("EmailConfirmed").TypeConverter<TestConverter>();
            Map(m => m.PhoneNumberConfirmed).Name("PhoneNumberConfirmed").TypeConverter<TestConverter>();
            Map(m => m.TwoFactorEnabled).Name("TwoFactorEnabled").TypeConverter<TestConverter>();
            Map(m => m.LockoutEnabled).Name("LockoutEnabled").TypeConverter<TestConverter>();
        }
    }

    public class TestConverter : DefaultTypeConverter
    {
        public override object ConvertFromString(string text, IReaderRow row, MemberMapData memberMapData)
        {
            return text.ToLower() switch
            {
                "t" => true,
                "f" => false,
                _ => base.ConvertFromString(text, row, memberMapData)
            };
        }
    }
}
