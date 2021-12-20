using JohnBlog.Models;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;
using JohnBlog.Data;
using JohnBlog.Enums;
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
            // When populating, make sure any FK are populated as well if used for display
            var homeVm = new HomeVm()
            {
                // TODO: add paging for blogs
               Blogs = _context.Blogs!
                   .Take(3)
                   .Include(b=>b.BlogUser)
                   .Include(m=>m.Posts)
                   .ToList(),
               Posts = _context.Posts!
                   .Where(p=>p.ReadyStatus == ReadyStatus.Production)
                   .OrderByDescending(p=> p.Created)
                   .Take(3)
                   .Include(b=>b.Blog)
                   .ToList(),
               Tags = _context.Tags!
                   .GroupBy(t=>t.TagText)
                   .OrderByDescending(tot =>tot.Count())
                   .Take(8)
                   .Select(t=>t.Key)
                   .ToList()!
            };
            return View(homeVm);
        }

        public IActionResult NoContentYet()
        {
            return View();
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