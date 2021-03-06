using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using JohnBlog.Data;
using JohnBlog.Models;
using Microsoft.AspNetCore.Identity;

namespace JohnBlog.Controllers
{
    public class CommentsController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<BlogUser> _userManager;

        public CommentsController(ApplicationDbContext context, UserManager<BlogUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        // GET: Comments
        public async Task<IActionResult> Index()
        {
            var applicationDbContext = _context.Comments!.Include(c => c.BlogUser).Include(c => c.Moderator)
                .Include(c => c.Post);
            return View(await applicationDbContext.ToListAsync());
        }

        // GET: Comments/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var comment = await _context.Comments!
                .Include(c => c.BlogUser)
                .Include(c => c.Moderator)
                .Include(c => c.Post)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (comment == null)
            {
                return NotFound();
            }

            return View(comment);
        }

        // GET: Comments/Create
        public IActionResult Create()
        {
            ViewData["BlogUserId"] = new SelectList(_context.Users, "Id", "Id");
            ViewData["ModeratorId"] = new SelectList(_context.Users, "Id", "Id");
            ViewData["PostId"] = new SelectList(_context.Posts, "Id", "Abstract");
            return View();
        }

        // POST: Comments/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("PostId,CommentText")] Comment comment, string slug)
        {
            if (ModelState.IsValid)
            {
                comment.BlogUserId = _userManager.GetUserId(User);
                comment.Created = DateTime.Now;

                _context.Add(comment);
                await _context.SaveChangesAsync();
                return RedirectToAction("Details", "Posts", new {slug});
                //return RedirectToAction(nameof(Index));
            }

            ViewData["BlogUserId"] = new SelectList(_context.Users, "Id", "Id", comment.BlogUserId);
            ViewData["ModeratorId"] = new SelectList(_context.Users, "Id", "Id", comment.ModeratorId);
            ViewData["PostId"] = new SelectList(_context.Posts, "Id", "Abstract", comment.PostId);
            return View(comment);
        }

        // GET: Comments/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var comment = await _context.Comments!.FindAsync(id);
            if (comment == null)
            {
                return NotFound();
            }

            ViewData["BlogUserId"] = new SelectList(_context.Users, "Id", "Id", comment.BlogUserId);
            ViewData["ModeratorId"] = new SelectList(_context.Users, "Id", "Id", comment.ModeratorId);
            ViewData["PostId"] = new SelectList(_context.Posts, "Id", "Abstract", comment.PostId);
            return View(comment);
        }

        // POST: Comments/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,CommentText")] Comment comment)
        {
            if (id != comment.Id) return NotFound();

            if (!ModelState.IsValid)
            {
                ViewData["BlogUserId"] = new SelectList(_context.Users, "Id", "Id", comment.BlogUserId);
                ViewData["ModeratorId"] = new SelectList(_context.Users, "Id", "Id", comment.ModeratorId);
                ViewData["PostId"] = new SelectList(_context.Posts, "Id", "Abstract", comment.PostId);
                return View(comment);
            }

            var updateComment = await _context.Comments!
                .Include(c => c.Post)
                .FirstOrDefaultAsync(c => c.Id == comment.Id);

            if (updateComment?.Post is null) return NotFound();
            try
            {
                updateComment.CommentText = comment.CommentText;
                updateComment.Updated = DateTime.Now;

                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!CommentExists(comment.Id)) return NotFound();
            }

            return RedirectToAction("Details", "Posts", new {slug = updateComment.Post.Slug}, "commentsSection");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Moderate(int id,
            [Bind("Id,CommentText,ModeratedBody,ModerationType")] Comment comment)
        {
            if (id != comment.Id) return NotFound();
            if (!ModelState.IsValid) return Problem("Comment.Moderate ModelState is not valid");
            var updateComment = await _context.Comments!
                .Include(c => c.Post)
                .FirstOrDefaultAsync(c => c.Id == comment.Id);
            if (updateComment?.Post is null) return NotFound();

            try
            {
                updateComment.ModeratedBody = comment.ModeratedBody;
                updateComment.ModerationType = comment.ModerationType;
                updateComment.Updated = DateTime.Now;
                updateComment.ModeratorId = _userManager.GetUserId(User);
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!CommentExists(comment.Id)) return NotFound();
            }

            return RedirectToAction("Details", "Posts", new {slug = updateComment.Post.Slug}, "commentsSection");
        }

        public async Task<IActionResult> MarkForDeletion(int? id, bool mark = true)
        {
            if (id is null) return NotFound();
            var comment = await _context.Comments!
                .Include(c => c.Post)
                .FirstOrDefaultAsync(c => c.Id == id);
            if (comment?.Post is null) return NotFound();
            
            try
            {
                comment.IsMarkedForDelete = mark;
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!CommentExists(comment.Id)) return NotFound();
            }

            return RedirectToAction("Details", "Posts", new {slug = comment.Post.Slug}, "commentsSection");
        }

        // GET: Comments/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var comment = await _context.Comments!
                .Include(c => c.BlogUser)
                .Include(c => c.Moderator)
                .Include(c => c.Post)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (comment == null)
            {
                return NotFound();
            }

            return View(comment);
        }

        // POST: Comments/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var comment = await _context.Comments!.FindAsync(id);
            _context.Comments.Remove(comment!);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool CommentExists(int id)
        {
            return _context.Comments!.Any(e => e.Id == id);
        }
    }
}