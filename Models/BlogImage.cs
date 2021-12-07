using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace JohnBlog.Models;

[Owned]
public class BlogImage
{
    public byte[]? ImageData { get; set; }
    public string? ContentType { get; set; }

    [NotMapped] public IFormFile? FormFile { get; set; }

    public string? GetImage
    {
        get
        {
            if (ImageData is null || ContentType is null) return null;
            return $"data:image/{ContentType};base64,{Convert.ToBase64String(ImageData)}";
        }
    }
}