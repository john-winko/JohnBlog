using JohnBlog.Enums;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Xml.Serialization;

namespace JohnBlog.Models
{
    public class Comment
    {
        public int Id { get; set; }
        public int? PostId { get; set; }
        public string? BlogUserId { get; set; }
        public string? ModeratorId { get; set; }

        [Required]
        [StringLength(500, ErrorMessage = "The {0} must be between {1} and {2} characters long.", MinimumLength = 2)]
        [Display(Name ="Comment")]
        public string? CommentText { get; set; }

        [Column(TypeName = "timestamp without time zone")]
        public DateTime Created { get; set; }
        
        [Column(TypeName = "timestamp without time zone")]
        public DateTime Updated { get; set; }

        [NotMapped]
        public bool IsModerated => ModeratorId != null;
        public bool IsMarkedForDelete { get; set; }

        [StringLength(500, ErrorMessage = "The {0} must be at least {2} and at most {1} characters long.", MinimumLength = 2)]
        [Display(Name = "Moderated")]
        public string? ModeratedBody { get; set; }

        public ModerationType ModerationType { get; set; }

        [XmlIgnore]
        public virtual Post? Post { get; set; }
        [XmlIgnore]
        public virtual BlogUser? BlogUser { get; set; }
        [XmlIgnore]
        public virtual BlogUser? Moderator { get; set; }
    }
}
