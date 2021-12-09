using System.Globalization;
using CsvHelper;
using CsvHelper.Configuration;
using CsvHelper.TypeConversion;
using JohnBlog.Data;
using JohnBlog.Models;
using Microsoft.AspNetCore.Identity;

namespace JohnBlog.Services
{
    public class DataService
    {
        private readonly ApplicationDbContext _dbContext;

        public DataService(ApplicationDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task SeedDatabaseAsync()
        {
            //await _dbContext.Database.EnsureDeletedAsync();
            await _dbContext.Database.EnsureCreatedAsync();
            await SeedDatabaseDefaultAsync();
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
            
            csv = new CsvReader(new StreamReader("D:/temp/AspNetRoles.csv"), CultureInfo.InvariantCulture);
            csv.Context.AutoMap<IdentityRole>();
            foreach (var bRecord in csv.GetRecords<IdentityRole>())
            {
                if (!_dbContext.Roles!.Any(p => p.Id == bRecord.Id))
                {
                    _dbContext.Roles!.Add(bRecord);
                }
                
            }
            
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
