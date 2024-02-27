using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using SocialBookmarkingReborn.Data;
using SocialBookmarkingReborn.Models;

namespace SocialBookmarkingReborn.Controllers
{
    public class CommentsController : Controller
    {
        // PASUL 10- useri si roluri

        private readonly ApplicationDbContext db;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;

        public CommentsController(
            ApplicationDbContext context,
            UserManager<ApplicationUser> userManager,
            RoleManager<IdentityRole> roleManager
            )
        {
            db = context;
            _userManager = userManager;
            _roleManager = roleManager;
        }


        // Adaugarea unui comentariu asociat unui articol in baza de date
        // [HttpPost]
        // public IActionResult New(Comment comm)
        //{
        //    comm.Date = DateTime.Now;

        //    if (ModelState.IsValid)
        //    {
        //        db.Comments.Add(comm);
        //        db.SaveChanges();
        //      return Redirect("/Bookmarks/Show/" + comm.BookmarkId);
        //    }

        //    else
        //    {
        //        return Redirect("/Bookmarks/Show/" + comm.BookmarkId);
        //    }

        // }

        // Stergerea unui comentariu din baza de date

        // Comentariul poate fi sters de utilzatorii cu rolul de administrator
        // sau de utilizatorul caruia ii apartine comentariul
        [HttpPost]
        [Authorize(Roles = ("RegisteredUser,Administrator"))]
        public IActionResult Delete(int id)
        {

            Comment comm = db.Comments.Find(id);

            if (comm.UserId == _userManager.GetUserId(User) || User.IsInRole("Administrator"))
            {
                db.Comments.Remove(comm);
                db.SaveChanges();
                return Redirect("/Bookmarks/Show/" + comm.BookmarkId);

            }
            else
            {
                TempData["message"] = "Nu aveti dreptul sa stergeti un comentariu care nu va apartine.";
                TempData["messageType"] = "alert-danger";
                // vezi daca merge si intreab o pe Maria ce parree aree
                return Redirect("/Bookmarks/Show/" + comm.BookmarkId);
            }

        }


        // Comentariul poate fi editat DOAR de proprietarul comentariului
        //[Authorize(Roles = ("RegisteredUser,Administrator"))]
         [Authorize(Roles = ("RegisteredUser"))]
        public IActionResult Edit(int id)
        {
            Comment comm = db.Comments.Find(id);
            if (comm.UserId == _userManager.GetUserId(User)) //|| User.IsInRole("Administrator"))
                return View(comm);
            else
            {
                TempData["message"] = "Nu aveti dreptul sa faceti modificari asupra unui comentariu care nu va apartine.";
                TempData["messageType"] = "alert-danger";
                // vezi daca merge si intreab o pe Maria ce parree aree
                return Redirect("/Bookmarks/Show/" + comm.BookmarkId);
            }
        }

        [HttpPost]
        //[Authorize(Roles = "RegisteredUser,Administrator")]
        [Authorize(Roles = "RegisteredUser")]
        public IActionResult Edit(int id, Comment requestComm)
        {
            Comment comm = db.Comments.Find(id);
            if (comm.UserId == _userManager.GetUserId(User)) // || User.IsInRole("Administrator"))
            {
                if (ModelState.IsValid)
                {
                    comm.Content = requestComm.Content;

                    db.SaveChanges();

                    return Redirect("/Bookmarks/Show/" + comm.BookmarkId);
                }
                else
                {
                    return View(requestComm);
                }
            }
            else
            {
                TempData["message"] = "Nu aveti dreptul sa faceti modificari asupra unui comentariu care nu va apartine.";
                TempData["messageType"] = "alert-danger";
                // vezi daca merge si intreab o pe Maria ce parree aree
                return Redirect("/Bookmarks/Show/" + comm.BookmarkId);

            }

        }



    }
}