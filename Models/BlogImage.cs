using System.ComponentModel.DataAnnotations.Schema;

namespace JohnBlog.Models
{
    public class BlogImage
    {
        public int Id { get; set; }
        public byte[] Image { get; set; }
        public string ContentType { get; set; }

        [NotMapped]
        public IFormFile? FormFile { get; set; }

    }
}
