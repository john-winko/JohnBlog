using System.Globalization;
using CsvHelper;
using CsvHelper.Configuration;
using CsvHelper.TypeConversion;
using JohnBlog.Data;
using JohnBlog.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace JohnBlog.Services
{
    public class DataService
    {
        private readonly ApplicationDbContext _dbContext;
        

        public DataService(ApplicationDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task SeedDatabaseAsync(bool reset = false)
        {
            if (reset) await _dbContext.Database.EnsureDeletedAsync();

            await _dbContext.Database.EnsureCreatedAsync();
            await SeedDatabaseDefaultAsync();

            // Have to reset the primary key sequences if we manually seed from scratch
            if (reset)
            {
                FileInfo file = new FileInfo(Directory.GetCurrentDirectory() + "/Data/FixPostgresSequence.sql");
                string script = await file.OpenText().ReadToEndAsync();
                var result = await _dbContext.Database.ExecuteSqlRawAsync(script);
            }
        }

        private async Task SeedDatabaseDefaultAsync()
        {
            // make sure our defaults exists
            // TODO: use generics for cleanup
            // TODO: save required .csv locally and change paths to relative location

            var csv = new CsvReader(new StreamReader("D:/temp/AspNetUsers.csv"), CultureInfo.InvariantCulture);
            csv.Context.RegisterClassMap<AspNetUsersMap>();

            foreach (var bRecord in csv.GetRecords<BlogUser>())
            {
                if (!_dbContext.Users!.Any(p => p.Id == bRecord.Id))
                {
                    _dbContext.Users!.Add(bRecord);
                }
            }

            await _dbContext.SaveChangesAsync();

            csv = new CsvReader(new StreamReader("D:/temp/AspNetRoles.csv"), CultureInfo.InvariantCulture);
            csv.Context.AutoMap<IdentityRole>();
            foreach (var bRecord in csv.GetRecords<IdentityRole>())
            {
                if (!_dbContext.Roles!.Any(p => p.Id == bRecord.Id))
                {
                    _dbContext.Roles!.Add(bRecord);
                }
            }

            await _dbContext.SaveChangesAsync();

            csv = new CsvReader(new StreamReader("D:/temp/AspNetUserRoles.csv"), CultureInfo.InvariantCulture);
            csv.Context.AutoMap<IdentityUserRole<string>>();
            foreach (var bRecord in csv.GetRecords<IdentityUserRole<string>>())
            {
                if (!_dbContext.UserRoles!.Any(p => p.UserId == bRecord.UserId))
                {
                    _dbContext.UserRoles!.Add(bRecord);
                }
            }

            await _dbContext.SaveChangesAsync();

            csv = new CsvReader(new StreamReader("D:/temp/Blogs.csv"), CultureInfo.InvariantCulture);
            csv.Context.RegisterClassMap<BlogMap>();
            foreach (var bRecord in csv.GetRecords<Blog>())
            {
                if (!_dbContext.Blogs!.Any(p => p.Id == bRecord.Id))
                {
                    _dbContext.Blogs!.Add(bRecord);
                }
            }

            await _dbContext.SaveChangesAsync();
        }
    }
    
    public sealed class AspNetUsersMap : ClassMap<BlogUser>
    {
        private AspNetUsersMap()
        {
            AutoMap(CultureInfo.InvariantCulture);

            // throwing error on nulls
            Map(m => m.LockoutEnd).Name("LockoutEnd").Ignore();

            // cvs storing t/f instead of recognized boolean value
            Map(m => m.EmailConfirmed).Name("EmailConfirmed").TypeConverter<TestConverter>();
            Map(m => m.PhoneNumberConfirmed).Name("PhoneNumberConfirmed").TypeConverter<TestConverter>();
            Map(m => m.TwoFactorEnabled).Name("TwoFactorEnabled").TypeConverter<TestConverter>();
            Map(m => m.LockoutEnabled).Name("LockoutEnabled").TypeConverter<TestConverter>();
        }

        internal static AspNetUsersMap CreateInstance()
        {
            return new AspNetUsersMap();
        }
    }

    public sealed class BlogMap : ClassMap<Blog>
    {
        private BlogMap()
        {
            Map(m => m.Id).Name("Id");
            Map(m => m.BlogUserId).Name("BlogUserId");
            Map(m => m.Name).Name("Name");
            Map(m => m.Description).Name("Description");
            Map(m => m.Created).Name("Created");
            Map(m => m.Updated).Name("Updated").Ignore(); //possible null date
            Map(m => m.BlogImage).Name("BlogImage");
        }

        internal static BlogMap CreateInstance()
        {
            return new BlogMap();
        }
    }

    public class TestConverter : DefaultTypeConverter
    {
        public static TestConverter CreateInstance()
        {
            return new TestConverter();
        }

        public override object ConvertFromString(string text, IReaderRow row, MemberMapData memberMapData)
        {
            return text.ToLower() switch
            {
                "t" => true,
                "true" => true,
                "f" => false,
                "false" => false,
                _ => base.ConvertFromString(text, row, memberMapData)
            };
        }
    }
}