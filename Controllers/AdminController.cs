using JohnBlog.Data;
using JohnBlog.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace JohnBlog.Controllers;

public class AdminController : Controller
{
    private readonly ApplicationDbContext _context;
    private readonly UserManager<BlogUser> _userManager;

    public class UserDetails : BlogUser
    {
        public string Roles { get; set; }
    }
    
    public AdminController(ApplicationDbContext context, UserManager<BlogUser> userManager)
    {
        _context = context;
        _userManager = userManager;
    }
    
// GET
    public async Task<IActionResult> GetRoles(string? roleName)
    {
        ViewData["RoleName"] = roleName ?? "All";
        if (roleName is null) return View(_context.Users.ToList());
        
        var users = await _userManager.GetUsersInRoleAsync(roleName);
        
        return View(users);
    }

    // public IActionResult Edit(string blogUserId)
    // {
    //     
    // }
    // TODO: add edit / delete functionality 
}