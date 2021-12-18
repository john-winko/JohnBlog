using JohnBlog.Data;
using JohnBlog.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace JohnBlog.Controllers;

public class AdminController : Controller
{
    private readonly ApplicationDbContext _context;
    private readonly UserManager<BlogUser> _userManager;

    public AdminController(ApplicationDbContext context, UserManager<BlogUser> userManager)
    {
        _context = context;
        _userManager = userManager;
    }

    // GET
    // TODO: use custom attribute editor
    [Authorize(Roles = "Administrator")]
    public IActionResult GetRoles()
    {
        var users = _context.Users
            .OrderByDescending(p => p.EmailConfirmed)
            .ThenByDescending(p => p.Email)
            .ToList();
        return View(users);
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
        var s = await _userManager.AddToRoleAsync(user, roleName);

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
    // TODO: create custom view/model that joins roles with users... injecting and repeatedly using usermanager = BAD!
}