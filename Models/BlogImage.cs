using Microsoft.EntityFrameworkCore;

namespace JohnBlog.Models;

[Owned]
public class BlogImage
{
    public byte[]? ImageData { get; set; }
    public string? ContentType { get; set; }
}