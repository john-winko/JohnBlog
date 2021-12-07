namespace JohnBlog;

public static class ExtensionMethods
{
    public static async Task<string?> ToDbString(this IFormFile? file)
    {
        // TODO: add a max size limiter
        if (file is null) return null; // guard statement
        await using var ms = new MemoryStream();
        await file.CopyToAsync(ms);
        return $"data:image/{file.ContentType};base64,{Convert.ToBase64String(ms.ToArray())}";
    }
}