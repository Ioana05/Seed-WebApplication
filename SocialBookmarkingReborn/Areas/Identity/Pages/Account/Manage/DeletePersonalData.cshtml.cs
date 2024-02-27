// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
#nullable disable

using System;
using System.ComponentModel.DataAnnotations;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using SocialBookmarkingReborn.Data;
using SocialBookmarkingReborn.Models;

namespace SocialBookmarkingReborn.Areas.Identity.Pages.Account.Manage
{
    public class DeletePersonalDataModel : PageModel
    {
        private readonly ApplicationDbContext db;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly ILogger<DeletePersonalDataModel> _logger;


        public DeletePersonalDataModel(
            ApplicationDbContext context,
            UserManager<ApplicationUser> userManager,
            SignInManager<ApplicationUser> signInManager,
            ILogger<DeletePersonalDataModel> logger)
        {
            db = context;
            _userManager = userManager;
            _signInManager = signInManager;
            _logger = logger;

        }

        /// <summary>
        ///     This API supports the ASP.NET Core Identity default UI infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        [BindProperty]
        public InputModel Input { get; set; }

        /// <summary>
        ///     This API supports the ASP.NET Core Identity default UI infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public class InputModel
        {
            /// <summary>
            ///     This API supports the ASP.NET Core Identity default UI infrastructure and is not intended to be used
            ///     directly from your code. This API may change or be removed in future releases.
            /// </summary>
            [Required]
            [DataType(DataType.Password)]
            public string Password { get; set; }
        }

        /// <summary>
        ///     This API supports the ASP.NET Core Identity default UI infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public bool RequirePassword { get; set; }

        public async Task<IActionResult> OnGet()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return NotFound($"Unable to load user with ID '{_userManager.GetUserId(User)}'.");
            }

            RequirePassword = await _userManager.HasPasswordAsync(user);
            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            /*
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return NotFound($"Unable to load user with ID '{_userManager.GetUserId(User)}'.");
            }

            RequirePassword = await _userManager.HasPasswordAsync(user);
            if (RequirePassword)
            {
                if (!await _userManager.CheckPasswordAsync(user, Input.Password))
                {
                    ModelState.AddModelError(string.Empty, "Incorrect password.");
                    return Page();
                }
            }

            var result = await _userManager.DeleteAsync(user);
            var userId = await _userManager.GetUserIdAsync(user);
            if (!result.Succeeded)
            {
                throw new InvalidOperationException($"Unexpected error occurred deleting user.");
            }

            await _signInManager.SignOutAsync();

            _logger.LogInformation("User with ID '{UserId}' deleted themselves.", userId);

            return Redirect("~/");
            */

            // pentru fiecare user va trebui sa stergem si categoriile pe care le are salvate, dar si 
            // bookmarkurile incarcate de el

            // stergem rand pe rand toate elementele care au legatura cu user-ul
            var currUser = db.ApplicationUsers.Include("Categories")
                                              .Include("Bookmark")
                                              .Include("Bookmark.Comments")
                                              .Include("Bookmark.Votes")
                                              .Include("Comments")
                                              .Include("Votes")
                                              .Include("Votes.Bookmark")
                                              .Where(user => user.Id == _userManager.GetUserId(User))
                                              .First();

            
            // stergem voturile
            if (currUser.Votes.Count > 0)
            {
                foreach(var vote in currUser.Votes) 
                {
                    // trebuie scazut counter-ul pt voturi al bookmark-ului
                    vote.Bookmark.NrVotes -= 1;
                    db.Votes.Remove(vote);
                }
            }

            // stergem comentariile
            if (currUser.Comments.Count > 0)
            {
                foreach(var comment in currUser.Comments)
                {
                    db.Comments.Remove(comment);
                }
            }

            // stergem bookmark-urile
            if (currUser.Bookmark.Count > 0) 
            { 
                foreach(var bookmark in currUser.Bookmark)
                {
                    db.Bookmarks.Remove(bookmark);
                }
            }

            // stergem categoriile
            if (currUser.Categories.Count > 0)
            {
                foreach(var category in currUser.Categories)
                {
                    db.Categories.Remove(category);
                }
            }

            // stergem user-ul
            db.ApplicationUsers.Remove(currUser);
            db.SaveChanges();
            return Redirect("/Identity/Account/Logout");
         
        }
    }
}
