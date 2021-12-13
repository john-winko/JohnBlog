using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Text;
using JohnBlog.Data;

namespace JohnBlog.Services;

public interface ISlugService
{
    public string GenerateUrlSlug(string title);
    public bool IsUnique(string slug);
}

public class SlugService : ISlugService
{
    private const int MAX_LENGTH = 80;
    private readonly ApplicationDbContext _context;

    public SlugService(ApplicationDbContext context)
    {
        _context = context;
    }

    public string GenerateUrlSlug(string? title)
    {
        if (title is null) return "";

        var len = title.Length;
        var prevDash = false;

        var sb = new StringBuilder(len);

        for (int i = 0; i < len; i++)
        {
            var c = title[i];
            switch (c)
            {
                case >= 'a' and <= 'z':
                case >= '0' and <= '9':
                    sb.Append(c);
                    prevDash = false;
                    break;
                case >= 'A' and <= 'Z':
                    // tricky way to convert to lowercase
                    sb.Append((char)(c | 32));
                    prevDash = false;
                    break;
                // Special characters
                case ' ':
                case ',':
                case '.':
                case '/':
                case '\\':
                case '-':
                case '_':
                case '=':
                {
                    if (!prevDash && sb.Length > 0)
                    {
                        sb.Append('-');
                        prevDash = true;
                    }
                    break;
                }
                // reserved characters
                case '#':
                {
                    if (i > 0)
                        if (title[i - 1] == 'C' || title[i - 1] == 'F')
                            sb.Append("-sharp");
                    break;
                }
                case '+':
                    sb.Append("-plus");
                    break;
                default:
                {
                    if (c >= 128)
                    {
                        var prevLen = sb.Length;
                        sb.Append(RemapInternationalCharToAscii(c));
                        if (prevLen != sb.Length) prevDash = false;
                    }

                    break;
                }
            }
            if (sb.Length == MAX_LENGTH) break;
        }
        // don't leave a dash as last char
        return prevDash ? sb.ToString()[..(sb.Length - 1)] : sb.ToString();
    }
    
    [SuppressMessage("ReSharper", "StringLiteralTypo")]
    private static string RemapInternationalCharToAscii(char c)
    {
        var s = c.ToString().ToLowerInvariant();
        if ("àåáâäãåą".Contains(s)) return "a";
        if ("èéêëę".Contains(s)) return "e";
        if ("ìíîïı".Contains(s)) return "i";
        if ("òóôõöøőð".Contains(s)) return "o";
        if ("ùúûüŭů".Contains(s)) return "u";
        if ("çćčĉ".Contains(s)) return "c";
        if ("żźž".Contains(s)) return "z";
        if ("śşšŝ".Contains(s)) return "s";
        if ("ñń".Contains(s)) return "n";
        if ("ýÿ".Contains(s)) return "y";
        if ("ğĝ".Contains(s)) return "g";
        
        return c switch
        {
            'ř' => "r",
            'ł' => "l",
            'đ' => "d",
            'ß' => "ss",
            'Þ' => "th",
            'ĥ' => "h",
            'ĵ' => "j",
            _ => ""
        };
    }

    public bool IsUnique(string slug)
    {
        Debug.Assert(_context.Posts != null, "_context.Posts != null");
        return !_context.Posts.Any(p => p.Slug == slug);
    }
}