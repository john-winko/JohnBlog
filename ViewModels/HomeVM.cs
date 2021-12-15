using JohnBlog.Models;

namespace JohnBlog.ViewModels;

public class HomeVm
{
    public IEnumerable<Blog>? Blogs { get; set; }
    public IEnumerable<Post>? Posts { get; set; }
    // TODO: Add tags
}