using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using SocialBookmarkingReborn.Data;
using SocialBookmarkingReborn.Models;
using System.Globalization;

namespace SocialBookmarkingReborn.Controllers
{
    public class VotesController : Controller
    {
        private readonly ApplicationDbContext db;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;

        public VotesController(
            ApplicationDbContext context,
            UserManager<ApplicationUser> userManager,
            RoleManager<IdentityRole> roleManager
            )
        {
            db = context;
            _userManager = userManager;
            _roleManager = roleManager;
        }

        // Administratorul nu are treaba nici cu voturile =)

        [Authorize(Roles = ("RegisteredUser"))]
        [HttpPost]
        public IActionResult New(int id, string name)
        {
            // ar trebui sa verificam daca nu am dat deja reactie
            // daca am dat sa se poata doar schimba => cheama edit de fapt
            // si sa ne-o putem retrage => cheama delete
            try
            {
                Bookmark bookmark = db.Bookmarks.Find(id);

                Vote vote = new Vote();
                //userId va fi cel inregistrat acum
                vote.BookmarkId = id;
                vote.Name = name;
                vote.UserId = _userManager.GetUserId(User);
                db.Votes.Add(vote);

                //vrem sa crestem counterul bookmark-ului potrivit;
                bookmark.NrVotes = bookmark.NrVotes + 1;
                db.SaveChanges();

                return Redirect("/Bookmarks/Show/" + bookmark.Id);
            }
            catch
            {
                ViewBag.ErrorMessage = "Something went wrong while trying " +
                                        "to register your vote =(";
                return View("Views/Shared/Error.cshtml");
            }

        }

        [Authorize(Roles = ("RegisteredUser"))]
        [HttpPost]
        public IActionResult Edit(int id, string name)
        {
            try
            {
                Vote vote = db.Votes.Find(id);

                //avem dreptul sa modificam votul?
                if (vote.UserId == _userManager.GetUserId(User))
                {
                    if (vote.Name == name) //daca am vrut sa modificam votul
                    {
                        // => il stergem de fapt 
                        /// pot sa fac redirect intr-un post? Nu pare

                        var bookmark = db.Bookmarks.Find(vote.BookmarkId);
                        //ca sa stiu unde sa fac redirect + sa-i scadem nrVoturi
                        bookmark.NrVotes = bookmark.NrVotes - 1;

                        db.Votes.Remove(vote);
                        db.SaveChanges();

                        return Redirect("/Bookmarks/Show/" + bookmark.Id);
                    }
                    else
                    {
                        //il modificam
                        vote.Name = name;
                        db.SaveChanges();

                        return Redirect("/Bookmarks/Show/" + vote.BookmarkId);
                    }
                }

                else
                {
                    ViewBag.ErrorMessage = "You don't have the right to " +
                                        "edit this bookmark!";
                    return View("Views/Shared/Error.cshtml");
                }

            }
            catch
            {
                ViewBag.ErrorMessage = "The vote that you're trying to " +
                                        "edit doesn't seem to exist!";
                return View("Views/Shared/Error.cshtml");
            }
        }
    }
}