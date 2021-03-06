using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Xml.Serialization;

namespace JohnBlog.Models
{
    public class Blog
    {
        public int Id { get; set; }
        public string BlogUserId { get; set; } = string.Empty;

        [Required]
        [StringLength(100, ErrorMessage = "The {0} must be between {1} and {2} characters long.", MinimumLength = 2)]
        public string Name { get; set; } = string.Empty;

        [Required]
        [StringLength(300, ErrorMessage = "The {0} must be between {1} and {2} characters long.", MinimumLength = 2)]
        public string Description { get; set; } = string.Empty;

        [Column(TypeName = "timestamp without time zone")]
        [Display(Name = "Created Date")]
        public DateTime Created { get; set; }

        [Column(TypeName = "timestamp without time zone")]
        [Display(Name = "Updated Date")]
        public DateTime? Updated { get; set; }
        
        [Display(Name = "Blog Image")]
        public string? BlogImage { get; set; } 

        // TODO: Implement NewImage as a binding rather than using return fields
        // [NotMapped]
        // public IFormFile? NewImage { get; set; }
        
        [Display(Name = "Author")]
        [XmlIgnore]
        public virtual BlogUser? BlogUser { get; set; }
        [XmlIgnore]
        public virtual ICollection<Post> Posts { get; set; } = new List<Post>();
    }
}
