using JohnBlog.Data;
using JohnBlog.Models;
using JohnBlog.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace JohnBlog.Controllers;

// TODO: use custom attribute editor
[Authorize(Roles = "Administrator")]
public class AdminController : Controller
{
    private readonly ApplicationDbContext _context;
    private readonly UserManager<BlogUser> _userManager;
    private readonly DataService _dataService;

    public AdminController(ApplicationDbContext context, UserManager<BlogUser> userManager, DataService dataService)
    {
        _context = context;
        _userManager = userManager;
        _dataService = dataService;
    }

    // GET
    public IActionResult GetRoles()
    {
        var users = GetBlogUsersWithRoles();

        return View(users);
    }

    private Dictionary<BlogUser, List<IdentityRole>> GetBlogUsersWithRoles()
    {
        // https://stackoverflow.com/questions/51004516/net-core-2-1-identity-get-all-users-with-their-associated-roles
        var res = _context.Users
            .OrderByDescending(p => p.EmailConfirmed)
            .ThenByDescending(p => p.Email)
            // left outer join for UserId->RoleIds, returns DefaultIfEmpty in the collectionSelector  
            .SelectMany(
                user => _context.UserRoles
                    .Where(userRoleMapEntry => user.Id == userRoleMapEntry.UserId)
                    .DefaultIfEmpty(),
                (user, roleMapEntry) => new {User = user, RoleMapEntry = roleMapEntry})
            // left outer join for RoleId->RoleNames
            .SelectMany(
                x => _context.Roles.Where(role => role.Id == x.RoleMapEntry.RoleId).DefaultIfEmpty(),
                (x, role) => new {x.User, Role = role})
            // runs the queries and sends us back into EF Core LINQ world
            .ToList()
            .Aggregate(
                // seed
                new Dictionary<BlogUser, List<IdentityRole>>(),
                // function
                (blogUser, roles) =>
                {
                    // safely ensure the user entry is configured
                    blogUser.TryAdd(roles.User, new List<IdentityRole>());
                    // ReSharper disable once ConditionIsAlwaysTrueOrFalse
                    if (null != roles.Role)
                    {
                        blogUser[roles.User].Add(roles.Role);
                    }

                    return blogUser;
                },
                // result
                x => x);
        return res;
    }

    public async Task<IActionResult> RemoveRole(string? blogUserId, string? roleName)
    {
        if (blogUserId is null || roleName is null) return NotFound();

        var user = await _userManager.FindByIdAsync(blogUserId);
        await _userManager.RemoveFromRoleAsync(user, roleName);
        return RedirectToAction("GetRoles");
    }

    public async Task<IActionResult> AddRole(string? blogUserId, string? roleName)
    {
        if (blogUserId is null || roleName is null) return NotFound();

        var user = await _userManager.FindByIdAsync(blogUserId);
        /*var s =*/ await _userManager.AddToRoleAsync(user, roleName);

        return RedirectToAction("GetRoles");
    }


    public async Task<IActionResult> DeleteUser(string? blogUserId)
    {
        if (blogUserId is null) return NotFound();
        var user = await _userManager.FindByIdAsync(blogUserId);
        return View(user);
    }

    [HttpPost, ActionName("DeleteUser")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ConfirmDeleteUser(string? id)
    {
        if (id is null) return NotFound();
        var user = await _userManager.FindByIdAsync(id);
        await _userManager.DeleteAsync(user);
        return RedirectToAction("GetRoles");
    }

    public IActionResult XmlFiles()
    {
        return View(XmlFileModel.Populate("/Data/SampleBlog/"));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> XmlFiles([Bind("FileNames,FileInfos,FormFile")] XmlFileModel xmlFileModel)
    {
        var formFile = xmlFileModel.FormFile;
        if (formFile is null) return View("XmlFiles");
        var fileUploadPath = Directory.GetCurrentDirectory() + $"/Data/SampleBlog/{formFile.FileName}";
        await using var fileStream = new FileStream(fileUploadPath, FileMode.OpenOrCreate);
        await formFile.CopyToAsync(fileStream);

        return RedirectToAction("XmlFiles");
    }

    public async Task<IActionResult> GenerateXmlFiles()
    {
        await _dataService.SaveAllXml();
        return RedirectToAction("XmlFiles");
    }

    public async Task<IActionResult> LoadXmlFiles()
    {
        await _dataService.SeedDatabaseAsync(true);
        return RedirectToAction("XmlFiles");
    }

    public class XmlFileModel
    {
        // ReSharper disable once MemberCanBePrivate.Global
        // ReSharper disable once PropertyCanBeMadeInitOnly.Global
        public List<FileInfo>? FileInfos { get; set; }
        // ReSharper disable once UnusedAutoPropertyAccessor.Global
        public IFormFile? FormFile { get; set; }

        // ReSharper disable once EmptyConstructor
        public XmlFileModel()
        {
        }

        public static XmlFileModel Populate(string directory)
        {
            var xmlFileModel = new XmlFileModel {FileInfos = new List<FileInfo>()};
            foreach (var file in Directory.EnumerateFiles(Directory.GetCurrentDirectory() + directory))
            {
                xmlFileModel.FileInfos.Add(new FileInfo(file));
            }

            return xmlFileModel;
        }
    }
}