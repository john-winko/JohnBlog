using System.Reflection;
using System.Xml.Serialization;
using JohnBlog.Data;
using JohnBlog.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Npgsql;

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

            // Have to reset the primary key sequences if we manually seed from scratch
            if (reset || NeedsSeeding())
            {
                await LoadAllXml();
                var file = new FileInfo(Directory.GetCurrentDirectory() + "/Data/FixPostgresSequence.sql");
                var script = await file.OpenText().ReadToEndAsync();
                /*var result =*/ await _dbContext.Database.ExecuteSqlRawAsync(script);
            }
        }

        private bool NeedsSeeding()
        {
            // TODO: add triggers to repopulate if users/database tables are too far out of whack or empty
            return !_dbContext.Users.Any(p => p.Email == "john.winko@gmail.com");;
        }

        public async Task LoadAllXml()
        {
            await _dbContext.AddRangeAsync(LoadFromXml<List<BlogUser>>());
            await _dbContext.AddRangeAsync(LoadFromXml<List<IdentityRole>>());
            await _dbContext.AddRangeAsync(LoadFromXml<List<IdentityUserRole<string>>>());
            await _dbContext.AddRangeAsync(LoadFromXml<List<Blog>>());
            await _dbContext.AddRangeAsync(LoadFromXml<List<Post>>());
            await _dbContext.AddRangeAsync(LoadFromXml<List<Comment>>());
            await _dbContext.AddRangeAsync(LoadFromXml<List<Tag>>());
            
            await _dbContext.SaveChangesAsync();
        }

        public async Task SaveAllXml()
        {
            SaveToXml(await _dbContext.UserRoles!.ToListAsync());
            SaveToXml(await _dbContext.Roles!.ToListAsync());
            SaveToXml(await _dbContext.Users!.ToListAsync());
            SaveToXml(await _dbContext.Blogs!.ToListAsync());
            SaveToXml(await _dbContext.Comments!.ToListAsync());
            SaveToXml(await _dbContext.Tags!.ToListAsync());
            SaveToXml(await _dbContext.Posts!.ToListAsync());
        }
        
        public void SaveToXml<T>(T obj)
        {
            // working with lists
            var className = typeof(T).GetProperty("Item")?.PropertyType.Name ?? typeof(T).FullName;

            var serializer = new XmlSerializer(typeof(T));
            using var sw = new StreamWriter($"Data/SampleBlog/{className}.xml");
            serializer.Serialize(sw, obj);
            sw.Close();
            typeof(T).GetCustomAttributes();
        }

        public T LoadFromXml<T>()
        {
            var className = typeof(T).GetProperty("Item")?.PropertyType.Name ?? typeof(T).FullName;
            var serializer = new XmlSerializer(typeof(T));
            using var fs = new FileStream($"Data/SampleBlog/{className}.xml", FileMode.Open);
            return (T) serializer.Deserialize(fs)!;
        }

        public static string BuildConnectionString(string databaseUrl)
        {
            //Provides an object representation of a uniform resource identifier (URI) and easy access to the parts of the URI.
            var databaseUri = new Uri(databaseUrl);
            var userInfo = databaseUri.UserInfo.Split(':');

            //Provides a simple way to create and manage the contents of connection strings used by the NpgsqlConnection class.
            var builder = new NpgsqlConnectionStringBuilder
            {
                Host = databaseUri.Host,
                Port = databaseUri.Port,
                Username = userInfo[0],
                Password = userInfo[1],
                Database = databaseUri.LocalPath.TrimStart('/'),
                SslMode = SslMode.Prefer,
                TrustServerCertificate = true
            };

            return builder.ToString();
        }
    }
}