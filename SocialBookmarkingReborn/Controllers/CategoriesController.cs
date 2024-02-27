using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SocialBookmarkingReborn.Data;
using SocialBookmarkingReborn.Models;

namespace SocialBookmarkingReborn.Controllers
{
    public class CategoriesController : Controller
    {
        private readonly ApplicationDbContext db;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;

        public CategoriesController(
            ApplicationDbContext context,
            UserManager<ApplicationUser> userManager,
            RoleManager<IdentityRole> roleManager
            )
        {
            db = context;
            _userManager = userManager;
            _roleManager = roleManager;
        }

        /* l-am mutat in show-ul de la user
        public IActionResult Index()
        {
            //vrem sa afisam categoriile user-ului conectat
            var categories = db.Categories
                               .Where(cat => cat.UserId == _userManager.GetUserId(User));
            ViewBag.Categories = categories;
            return View();
        }*/

        // Administratorul nu are treaba cu categoriile

        [Authorize(Roles = "RegisteredUser")]
        public IActionResult Show(int id)
        {
            try
            {
                Category cat = db.Categories.Where(cat => cat.Id == id)
                                            .Include("BookmarkCategories")
                                            .Include("BookmarkCategories.Bookmark")
                                            .Include("BookmarkCategories.Bookmark.User")
                                            .First();

                // ar fi trebuit poate introdusa ca si proprietate...
                int?[] bookmarks = (from bkmk in cat.BookmarkCategories
                                    select bkmk.BookmarkId).ToArray();

                ViewBag.NrBookmarks = bookmarks.Length;
                // trimitem pt butonul de stergere de pe articolele 
                ViewBag.UserCurent = _userManager.GetUserId(User);

                return View(cat);
            }
            catch
            {
                ViewBag.ErrorMessage = "The category you are tryng to access" +
                                        " doesn't seem to exist!";
                return View("Views/Shared/Error.cshtml");
            }
        }

        [Authorize(Roles = "RegisteredUser")]
        public IActionResult New(int? id) //id-ul bookmark-ului
        {
            // daca id != null => vrem sa adaugam bookmark-ul cu id-ul primit
            // in categorie dupa ce o chemam
            ViewBag.BookmarkId = -1;

            if (id.HasValue)
                ViewBag.BookmarkId = id;

            Category category = new Category();
            return View(category);
        }

        [Authorize(Roles = "RegisteredUser")]
        [HttpPost]
        public IActionResult New([FromForm] Category category, [FromForm] int bkmkId, [FromForm] bool visible)
        {
            //setam user-ul ca fiind cel curent
            category.UserId = _userManager.GetUserId(User);

            //daca switch-ul pt vizibilitate este checked
            if (visible == true)
            {
                category.Visibility = true;
            }
            else
            {
                category.Visibility = false;
            }

            if (ModelState.IsValid)
            {
                db.Categories.Add(category);
                db.SaveChanges();

                //daca trebuie si sa adaugam un bookmark o facem
                if (bkmkId > 0)
                {
                    BookmarkCategory bkmkcat = new BookmarkCategory();
                    bkmkcat.BookmarkId = bkmkId;
                    bkmkcat.CategoryId = category.Id;
                    db.BookmarkCategories.Add(bkmkcat);
                    db.SaveChanges();

                    //ne intoarcem inapoi la bookmark
                    return Redirect("/Bookmarks/Show/" + bkmkcat.BookmarkId);
                }

                else return Redirect("/ApplicationUsers/Show/" + category.UserId);
            }
            else
            {
                return View(category);
            }
        }

        [Authorize(Roles = ("RegisteredUser"))]
        public IActionResult Edit(int id)
        {
            Category cat = db.Categories.Find(id);

            // daca suntem creatorii categoriei
            if (cat.UserId == _userManager.GetUserId(User))
            {
                return View(cat);
            }
            else
            {
                ViewBag.ErrorMessage = "You don't have the right to " +
                                        "edit this category!";
                return View("Views/Shared/Error.cshtml");
            }
        }

        [Authorize(Roles = "RegisteredUser")]
        [HttpPost]
        public IActionResult Edit(int id, Category requestCategory, [FromForm] bool visible)
        {
            Category cat = db.Categories.Find(id);

            // daca suntem creatorii categoriei
            if (cat.UserId == _userManager.GetUserId(User))
            {
                if (ModelState.IsValid)
                {
                    cat.Name = requestCategory.Name;
                    cat.Description = requestCategory.Description;
                    cat.Visibility = visible;
                    db.SaveChanges();
                    // ne intoarcem la pagina de profil a user-ului curent
                    return Redirect("/Categories/Show/" + id);
                }
                else
                {
                    return View(requestCategory);
                }
            }
            else
            {
                ViewBag.ErrorMessage = "You don't have the right to " +
                                        "edit this category!";
                return View("Views/Shared/Error.cshtml");
            }
        }

        [Authorize(Roles = "RegisteredUser")]
        [HttpPost]
        public IActionResult Delete(int id)
        {
            Category cat = db.Categories.Find(id);
            // daca suntem creatorii categoriei
            if (cat.UserId == _userManager.GetUserId(User))
            {
                db.Categories.Remove(cat);
                db.SaveChanges();
                // ne intoarcem la pagina de profil a user-ului curent
                return Redirect("/ApplicationUsers/Show/" + _userManager.GetUserId(User));
            }
            else
            {
                ViewBag.ErrorMessage = "You don't have the right to " +
                                        "delete this category!";
                return View("Views/Shared/Error.cshtml");
            }
        }
    }
}