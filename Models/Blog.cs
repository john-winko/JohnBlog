using System.ComponentModel.DataAnnotations;

namespace JohnBlog.Models
{
    public class Blog
    {
        public int Id { get; set; }
        public string BlogUserID { get; set; }

        [Required]
        public string Name { get; set; }    
         public string Description { get; set; }  
        public DateTime CreatedDate { get; set; } 
        public DateTime UpdatedDate { get; set; }
        public BlogImage Image { get; set; }

        public virtual BlogUser BlogUser { get; set; }
    }
}
