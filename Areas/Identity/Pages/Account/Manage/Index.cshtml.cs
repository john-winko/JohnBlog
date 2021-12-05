// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
#nullable disable

using System;
using System.ComponentModel.DataAnnotations;
using System.Text.Encodings.Web;
using System.Threading.Tasks;
using JohnBlog.Models;
using JohnBlog.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace JohnBlog.Areas.Identity.Pages.Account.Manage
{
    public class IndexModel : PageModel
    {
        private readonly UserManager<BlogUser> _userManager;
        private readonly SignInManager<BlogUser> _signInManager;
        private readonly IImageService _imageService;

        public IndexModel(
            UserManager<BlogUser> userManager,
            SignInManager<BlogUser> signInManager, 
            IImageService imageService)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _imageService = imageService;
        }

        /// <summary>
        ///     This API supports the ASP.NET Core Identity default UI infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public string Username { get; set; }
        public IFormFile FormFile { get; set; }
    
        /// <summary>
        ///     This API supports the ASP.NET Core Identity default UI infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        [TempData]
        public string StatusMessage { get; set; }

        /// <summary>
        ///     This API supports the ASP.NET Core Identity default UI infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        [BindProperty]
        public BlogUser Input { get; set; }

        /// <summary>
        ///     This API supports the ASP.NET Core Identity default UI infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>

        private async Task LoadAsync(BlogUser user)
        {
            Username = await _userManager.GetUserNameAsync(user);
            Input = user;
        }

        public async Task<IActionResult> OnGetAsync()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return NotFound($"Unable to load user with ID '{_userManager.GetUserId(User)}'.");
            }

            await LoadAsync(user);
            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return NotFound($"Unable to load user with ID '{_userManager.GetUserId(User)}'.");
            }

            if (!ModelState.IsValid)
            {
                await LoadAsync(user);
                return Page();
            }

            // Only over write if not null
            user.PhoneNumber = Input.PhoneNumber ?? user.PhoneNumber;
            // TODO update image service to return a BlogImage type to save
            user.BlogImage.ImageData = await _imageService.EncodeImageAsync(FormFile) ?? user.BlogImage.ImageData;
            user.BlogImage.ContentType = _imageService.ContentType(FormFile) ?? user.BlogImage.ContentType;

            user.FirstName = Input.FirstName;
            user.LastName = Input.LastName;
            user.Title = Input.Title;
            user.Blurb = Input.Blurb;
            user.FacebookUrl = Input.FacebookUrl;
            user.TwitterUrl = Input.TwitterUrl;
            user.LinkedInUrl = Input.LinkedInUrl;
            
            await _userManager.UpdateAsync(user);
            
            await _signInManager.RefreshSignInAsync(user);
            StatusMessage = "Your profile has been updated";
            return RedirectToPage();
        }
    }
}
