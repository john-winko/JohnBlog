using JohnBlog.Models;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;
using JohnBlog.Data;
using JohnBlog.ViewModels;
using Microsoft.EntityFrameworkCore;

namespace JohnBlog.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly ApplicationDbContext _context;

        public HomeController(ILogger<HomeController> logger, ApplicationDbContext context)
        {
            _logger = logger;
            _context = context;
        }

        public IActionResult Index()
        {
            var homeVm = new HomeVm()
            {
                // TODO: add paging for blogs
               Blogs = _context.Blogs!
                   .Take(3)
                   .ToList(),
               Posts = _context.Posts!
                   .OrderByDescending(p=> p.Created)
                   .Take(3)
                   .ToList()
            };
            return View(homeVm);
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}