using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;

namespace JohnBlog.Models
{
    public class Blog
    {
        public int Id { get; set; }
        public string BlogUserID { get; set; } = string.Empty;

        [Required]
        [StringLength(100, ErrorMessage = "The {0} must be between {1} and {2} characters long.", MinimumLength = 2)]
        public string Name { get; set; } = string.Empty;

        [Required]
        [StringLength(100, ErrorMessage = "The {0} must be between {1} and {2} characters long.", MinimumLength = 2)]
        public string Description { get; set; } = string.Empty;


        [DataType(DataType.Date)]
        [Display(Name = "Created Date")]
        public DateTime Created { get; set; }

        [DataType(DataType.Date)]
        [Display(Name = "Updated Date")]
        public DateTime? Updated { get; set; }

        [BindProperty]
        public BlogImage? Image { get; set; }

        [Display(Name = "Author")]
        public virtual BlogUser? BlogUser { get; set; } 
        public virtual ICollection<Post> Posts { get; set; } = new List<Post>();
    }
}
