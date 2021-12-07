using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using JohnBlog.Data;
using JohnBlog.Enums;
using JohnBlog.Models;
using JohnBlog.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;

namespace JohnBlog.Controllers
{
    public class BlogsController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<BlogUser> _userManager;
        private readonly IImageService _imageService;

        public BlogsController(ApplicationDbContext context, UserManager<BlogUser> userManager, IImageService imageService)
        {
            _context = context;
            _userManager = userManager;
            _imageService = imageService;
        }

        // GET: Blogs
        public async Task<IActionResult> Index()
        {
            var applicationDbContext = _context.Blogs!.Include(b => b.BlogUser);
            return View(await applicationDbContext.ToListAsync());
        }

        // GET: Blogs/Create
        [Authorize(Roles = $"{nameof(BlogRole.Administrator)},{nameof(BlogRole.Author)}")]
        public IActionResult Create()
        {
            //ViewData["BlogUserId"] = new SelectList(_context.Users, "Id", "Id");
            return View();
        }

        // POST: Blogs/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,Name,Description,BlogImage")] Blog blog)
        {
            if (ModelState.IsValid)
            {
                blog.BlogUserId = _userManager.GetUserId(User);
                blog.Created = DateTime.Now;
                if (blog.BlogImage.FormFile is not null)
                {
                    blog.BlogImage.ContentType = _imageService.ContentType(blog.BlogImage.FormFile);
                    blog.BlogImage.ImageData = await _imageService.EncodeImageAsync(blog.BlogImage.FormFile);
                }
                
                
                _context.Add(blog);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            ViewData["BlogUserId"] = new SelectList(_context.Users, "Id", "Id", blog.BlogUserId);
            return View(blog);
        }

        // GET: Blogs/Edit/5
        [Authorize]
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var blog = await _context.Blogs!.FindAsync(id);
            if (blog == null)
            {
                return NotFound();
            }
            ViewData["BlogUserId"] = new SelectList(_context.Users, "Id", "Id", blog.BlogUserId);
            return View(blog);
        }

        // POST: Blogs/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,BlogUserId,Name,Description,Created,BlogImage")] Blog blog)
        {
            if (id != blog.Id) return NotFound();
            if (!ModelState.IsValid) return View(blog);

            try
            {
                if (blog.BlogImage.FormFile is not null)
                {
                    blog.BlogImage.ContentType = _imageService.ContentType(blog.BlogImage.FormFile);
                    blog.BlogImage.ImageData = await _imageService.EncodeImageAsync(blog.BlogImage.FormFile);
                }
                blog.Updated = DateTime.Now;
                    
                _context.Update(blog);
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!BlogExists(blog.Id))
                {
                    return NotFound();
                }
                throw;
            }
            return RedirectToAction(nameof(Index));
        }

        // GET: Blogs/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var blog = await _context.Blogs!
                .Include(b => b.BlogUser)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (blog == null)
            {
                return NotFound();
            }

            return View(blog);
        }

        // POST: Blogs/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var blog = await _context.Blogs!.FindAsync(id);
            _context.Blogs.Remove(blog!);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool BlogExists(int id)
        {
            return _context.Blogs!.Any(e => e.Id == id);
        }
    }
}
