using JohnBlog.Enums;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace JohnBlog.Models
{
    public class Post
    {
        public int Id { get; set; }

        [Display(Name = "Blog Name")]
        public int BlogId { get; set; }

        // TODO: rename to AuthorID to make more sense
        // Set programatically so using required field makes issues on validation, must make nullable since it is blank until saved
        // [Display(Name = "Blog UserID")]
        public string? BlogUserId { get; set; }

        [Required]
        [StringLength(50, ErrorMessage = "The {0} must be at least {2} and at most {1} characters long.", MinimumLength = 2)]
        public string? Title { get; set; }

        [Required]
        [StringLength(200, ErrorMessage = "The {0} must be at least {2} and at most {1} characters long.", MinimumLength = 5)]
        public string? Abstract { get; set; }

        [Required]
        public string? Content { get; set; }

        [Column(TypeName = "timestamp without time zone")]
        [Display(Name = "Created Date")]
        public DateTime Created { get; set; }

        [Column(TypeName = "timestamp without time zone")]
        [Display(Name = "Updated Date")]
        public DateTime? Updated { get; set; }

        public ReadyStatus ReadyStatus { get; set; }

        // EF Core does not support unique constraint but EF6+ does... workaround?
        public string? Slug { get; set; }

        public string? BlogImage { get; set; }

        // Navigation properties
        public virtual Blog? Blog { get; set; }
        public virtual BlogUser? BlogUser { get; set; }

        public virtual ICollection<Tag> Tags { get; set; } = new HashSet<Tag>();
        public virtual ICollection<Comment> Comments { get; set; } = new HashSet<Comment>();
    }
}
