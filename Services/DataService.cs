using System.Reflection;
using System.Xml.Serialization;
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

        public async Task SeedDatabaseAsync(bool reset = true)
        {
            if (reset) await _dbContext.Database.EnsureDeletedAsync();

            await _dbContext.Database.EnsureCreatedAsync();
            //SaveAllXml();

            // Have to reset the primary key sequences if we manually seed from scratch
            if (reset)
            {
                await LoadAllXml();
                //await SeedDatabaseDefaultAsync();
                FileInfo file = new FileInfo(Directory.GetCurrentDirectory() + "/Data/FixPostgresSequence.sql");
                string script = await file.OpenText().ReadToEndAsync();
                var result = await _dbContext.Database.ExecuteSqlRawAsync(script);
            }
        }

        private async Task LoadAllXml()
        {
            _dbContext.AddRange(LoadFromXml<List<BlogUser>>());
            _dbContext.AddRange(LoadFromXml<List<IdentityRole>>());
            _dbContext.AddRange(LoadFromXml<List<IdentityUserRole<string>>>());
            _dbContext.AddRange(LoadFromXml<List<Blog>>());
            _dbContext.AddRange(LoadFromXml<List<Post>>());
            _dbContext.AddRange(LoadFromXml<List<Comment>>());
            _dbContext.AddRange(LoadFromXml<List<Tag>>());
            
            await _dbContext.SaveChangesAsync();
        }

        public void SaveAllXml()
        {
            SaveToXml(_dbContext.UserRoles!.ToList());
            SaveToXml(_dbContext.Roles!.ToList());
            SaveToXml(_dbContext.Users!.ToList());
            SaveToXml(_dbContext.Blogs!.ToList());
            SaveToXml(_dbContext.Comments!.ToList());
            SaveToXml(_dbContext.Tags!.ToList());
            SaveToXml(_dbContext.Posts!.ToList());
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
    }
}