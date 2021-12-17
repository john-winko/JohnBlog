using JohnBlog.Data;
using JohnBlog.Models;
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
    public IActionResult Index()
    {
        var users = _context.Users;
        return View(users);
    }

    // public IActionResult Edit(string blogUserId)
    // {
    //     
    // }
    // TODO: add edit / delete functionality 
}