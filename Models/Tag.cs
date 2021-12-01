using System.ComponentModel.DataAnnotations;

namespace JohnBlog.Models
{
    public class Tag
    {
        public int Id { get; set; }
        public int PostId { get; set; }

        [Required]
        [StringLength(25, ErrorMessage = "The {0} must be between {1} and {2} characters long", MinimumLength = 2)]
        [Display(Name ="Tag")]
        public string? TagText { get; set; }
        public virtual Post? Post { get; set; }

    }
}
