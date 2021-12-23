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
    public class PostsController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<BlogUser> _userManager;
        private readonly SlugService _slugService;

        public PostsController(ApplicationDbContext context, UserManager<BlogUser> userManager, SlugService slugService)
        {
            _context = context;
            _userManager = userManager;
            _slugService = slugService;
        }

        // GET: Posts
        public async Task<IActionResult> Index()
        {
            var applicationDbContext = _context.Posts!
                .OrderByDescending(p => p.Created)
                .Include(p => p.Blog)
                .Include(p => p.BlogUser);
            ViewData["BlogUserId"] = applicationDbContext.FirstOrDefault()!.BlogUserId;
            return View(await applicationDbContext.ToListAsync());
        }

        // GET: Posts
        public async Task<IActionResult> PostsByBlogIndex(int? blogId)
        {
            var applicationDbContext = _context.Posts!
                .Include(p => p.Blog)
                .Include(p => p.BlogUser)
                .Where(p => p.BlogId == blogId)
                .OrderBy(p => p.Created);
            return applicationDbContext.Any()
                ? View("Index", await applicationDbContext.ToListAsync())
                : View("NoPosts");
        }

        public async Task<IActionResult> PostsByTag(string tag)
        {
            // TODO: expand this further with its own view (show tag at top etc)
            var applicationDbContext = _context.Posts!
                .Include(p => p.Blog)
                .Include(p => p.BlogUser)
                .Where(p => p.Tags.Any(t => t.TagText == tag))
                .Where(p => p.ReadyStatus == ReadyStatus.Production);
            return applicationDbContext.Any()
                ? View("Index", await applicationDbContext.ToListAsync())
                : View("NoPosts");
        }

        public IActionResult NoPosts()
        {
            return View();
        }

        public async Task<IActionResult> Details(string? slug)
        {
            if (slug is null) return NotFound();

            var post = await _context.Posts!
                .Include(p => p.Blog)
                .Include(p => p.BlogUser)
                .Include(p => p.Tags)
                .Include(p => p.Comments)
                .ThenInclude(c => c.BlogUser)
                .Include(p => p.Comments)
                .ThenInclude(c => c.Moderator)
                .FirstOrDefaultAsync(m => m.Slug == slug);

            if (post is null) return NotFound();

            return View(post);
        }

        // GET: Posts/Create
        [Authorize]
        public IActionResult Create()
        {
            // Check if they are assigned any blogs
            var b = _context.Blogs!
                .Where(b => b.BlogUserId == _userManager.GetUserId(User));
            if (!b.Any()) return Problem("You have not been assigned a blog to post to");

            // ViewData["BlogId"] = new SelectList(_context.Blogs, "Id", "Name");
            ViewData["BlogUserId"] = _userManager.GetUserId(User);
            ViewData["BlogIds"] = new SelectList(b.ToList(), "Id", "Name");

            return View();
        }

        // POST: Posts/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(
            [Bind("Id,BlogUserId,BlogId,Title,Abstract,Content,ReadyStatus")]
            Post post, IFormFile? formFile, List<string> tagEntries)
        {
            if (ModelState.IsValid)
            {
                post.Created = DateTime.Now;
                post.BlogUserId = _userManager.GetUserId(User);
                post.BlogImage = await formFile.ToDbString() ?? post.BlogImage;

                var slug = _slugService.GenerateUrlSlug(post.Title);

                // Error check slug before adding
                if (string.IsNullOrEmpty(slug)) ModelState.AddModelError("", "Slug generated was empty");
                if (!_slugService.IsUnique(slug))
                    ModelState.AddModelError("Title", "Same title already exists for a post");
                if (!ModelState.IsValid) return View(post);

                post.Slug = slug;

                foreach (var tagEntry in tagEntries)
                {
                    post.Tags.Add(new Tag {PostId = post.Id, TagText = tagEntry.ToUpper()});
                }

                _context.Add(post);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }

            ViewData["BlogId"] = new SelectList(_context.Blogs, "Id", "Name", post.BlogId);
            return View(post);
        }

        // GET: Posts/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var post = await _context.Posts!
                .Include(p => p.Tags)
                .FirstOrDefaultAsync(p => p.Id == id);
            if (post == null)
            {
                return NotFound();
            }

            ViewData["BlogId"] = new SelectList(_context.Blogs, "Id", "Description", post.BlogId);
            ViewData["BlogUserId"] = new SelectList(_context.Users, "Id", "Id", post.BlogUserId);
            return View(post);
        }

        // POST: Posts/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id,
            [Bind("Id,BlogId,BlogUserId,Title,Abstract,Content,Created,Updated,ReadyStatus,Slug")]
            Post post, List<string> tagEntries)
        {
            if (id != post.Id) return NotFound();

            if (ModelState.IsValid)
            {
                try
                {
                    var postUpdate = _context.Posts!
                        .Include(p => p.Tags)
                        .FirstOrDefault(p => p.Id == post.Id);
                    postUpdate!.Updated = DateTime.Now;

                    // Remove all tags 
                    postUpdate.Tags.Clear();
                    await _context.SaveChangesAsync();

                    // then add tagEntries posted even if they are the same
                    foreach (var tagEntry in tagEntries)
                    {
                        postUpdate.Tags.Add(new Tag {PostId = post.Id, TagText = tagEntry.ToUpper()});
                    }

                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!PostExists(post.Id)) return NotFound();
                    throw;
                }

                return RedirectToAction(nameof(PostsByBlogIndex), new {blogId = post.BlogId});
            }

            ViewData["BlogId"] = new SelectList(_context.Blogs, "Id", "Description", post.BlogId);
            ViewData["BlogUserId"] = new SelectList(_context.Users, "Id", "Id", post.BlogUserId);
            return View(post);
        }

        // GET: Posts/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var post = await _context.Posts!
                .Include(p => p.Blog)
                .Include(p => p.BlogUser)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (post == null)
            {
                return NotFound();
            }

            return View(post);
        }

        // POST: Posts/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var comments = _context.Comments!.Where(p => p.PostId == id);
            _context.Comments!.RemoveRange(comments);
            await _context.SaveChangesAsync();
            
            var post = await _context.Posts!.FindAsync(id);
            _context.Posts!.Remove(post!);
            await _context.SaveChangesAsync();
            
            return RedirectToAction(nameof(Index));
        }

        private bool PostExists(int id)
        {
            return _context.Posts!.Any(e => e.Id == id);
        }
    }
}