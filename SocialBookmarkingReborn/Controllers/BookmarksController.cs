using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SocialBookmarkingReborn.Data;
using SocialBookmarkingReborn.Models;
using System.Text.RegularExpressions;

namespace SocialBookmarkingReborn.Controllers
{
    public class BookmarksController : Controller
    {
        private readonly ApplicationDbContext db;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        private IWebHostEnvironment _env;

        public BookmarksController(
            ApplicationDbContext context,
            UserManager<ApplicationUser> userManager,
            RoleManager<IdentityRole> roleManager,
            IWebHostEnvironment env
            )
        {
            db = context;
            _userManager = userManager;
            _roleManager = roleManager;
            _env = env;
        }


        // afisam bookmark-urile dupa data / dupa numarul de voturi 
        public IActionResult Index(bool? orderByPopular)
        {
            // Alegem sa afisam 9 bookmarks pe pagina
            int _perPage = 9;


            /////////////////   FILTRE   ////////////////////////
            var bookmarks = db.Bookmarks.Include("User")
                                      .OrderByDescending(bkmk => bkmk.NrVotes);

            //implicit ordonam in functie de data
            if (!orderByPopular.HasValue || orderByPopular == false)
            {
                bookmarks = db.Bookmarks.Include("User")
                                      .OrderByDescending(bkmk => bkmk.Date);
            }

            //daca orderByPopular e true ramane ordonat dupa popularitate




            /////////////////   MOTORUL DE CAUTARE  //////////////////////
            var search = "";
            if (Convert.ToString(HttpContext.Request.Query["search"]) != null)
            {
                // eliminam spatiile libere
                search = Convert.ToString(HttpContext.Request.Query["search"]).Trim();

                // Cautare in bookmark-uri (Title si Content)
                /// cautare in categorii (???)
                List<int> bookmarkIds = db.Bookmarks.Where
                                        (bkmk => bkmk.Title.Contains(search)
                                              || bkmk.Description.Contains(search)
                                        ).Select(bkmk => bkmk.Id).ToList();

                // Lista bookmark-urilor care contin cuvantul cautat
                bookmarks = db.Bookmarks.Where(bkmk => bookmarkIds.Contains(bkmk.Id))
                                        .Include("User")
                                        .OrderBy(bkmk => bkmk.Title);
                //ordonam alfabetic dupa titlu
            }

            ViewBag.SearchString = search;




            /////////////////   PAGINARE   //////////////////////

            // verificam cate bookmarks avem
            int totalItems = bookmarks.Count();

            // Se preia pagina curenta din View-ul asociat
            // Numarul paginii este valoarea parametrului page din ruta
            var currentPage = Convert.ToInt32(HttpContext.Request.Query["page"]);

            // offset =  cu numarul de bookmarks care au fost deja afisate
            //           pe paginile anterioare

            var offset = 0;
            if (!currentPage.Equals(0))
            {
                offset = (currentPage - 1) * _perPage;
            }

            // luam bookmarks-urile corespunzatoare pt pagina curenta
            var paginatedBookmarks = bookmarks.Skip(offset).Take(_perPage);

            // vrem si numarul ultimei pagini
            // - impartire cu nr rationale rotunjita superior
            ViewBag.lastPage = Math.Ceiling((float)totalItems / (float)_perPage);

            ViewBag.Bookmarks = paginatedBookmarks;



            if (search != "")
            {
                if (orderByPopular == true)
                {
                    ViewBag.PaginationBaseUrl = "/home/true/?search="
                                                + search + "&page";
                }
                else
                {
                    ViewBag.PaginationBaseUrl = "/home/?search="
                                                + search + "&page";
                }
            }
            else
            {
                if (orderByPopular == true)
                {
                    ViewBag.PaginationBaseUrl = "/home/true/?page";
                }
                else
                {
                    ViewBag.PaginationBaseUrl = "/home/?page";
                }
            }


            return View();
        }



        public IActionResult Show(int? id)
        {
            try
            {
                Bookmark bookmark = (from bkmk in db.Bookmarks.Include("Votes")
                                                              .Include("User")
                                                              .Include("Votes.User")
                                                              .Include("Comments")
                                                              .Include("Comments.User")
                                     where bkmk.Id == id
                                     select bkmk).First();

                // luam categoriile userului pt a putea salva bookmarkul intr-una dintre ele

                ViewBag.Categories = db.Categories
                                        .Where(cat =>
                                               cat.UserId == _userManager.GetUserId(User));

                // luam id-urile categoriilor in care l-a salvat deja

                int?[] categoriiSalvate = (from bkmkcat in db.BookmarkCategories
                                           where bkmkcat.BookmarkId == bookmark.Id
                                           select bkmkcat.CategoryId).ToArray();

                ViewBag.CategoriiSalvate = categoriiSalvate;

                // verificam aici daca userul a dat sau nu reactie la bookmarkul
                // pe care il vizualizeaza

                var vote = db.Votes.Where(v => v.BookmarkId == bookmark.Id)
                                   .Where(v => v.UserId == _userManager.GetUserId(User));

                // daca am dat
                if (vote.Any())
                {
                    ViewBag.Vote = "true";
                    ViewBag.VoteId = vote.First().Id;

                    //vom vrea sa afisam diferit reactia pe care 
                    //am dat-o 
                    ViewBag.Heart = "-outline";
                    ViewBag.Haha = "-outline";
                    ViewBag.Like = "-outline";
                    ViewBag.Smart = "-outline";
                    ViewBag.Wow = "-outline";

                    string name = vote.First().Name;
                    ViewBag.VoteName = name;

                    // ii setam si culoarea potrivita
                    switch (name)
                    {
                        case "heart-fill":
                            {
                                ViewBag.Color = "danger";
                                ViewBag.Heart = "";
                                break;
                            }
                        case "lightbulb-fill":
                            {
                                ViewBag.Color = "info";
                                ViewBag.Smart = "";
                                break;
                            }
                        case "hand-thumbs-up-fill":
                            {
                                ViewBag.Color = "primary";
                                ViewBag.Like = "";
                                break;
                            }
                        case "emoji-laughing-fill":
                            {
                                ViewBag.Color = "warning";
                                ViewBag.Haha = "";
                                break;
                            }
                        case "emoji-surprise-fill":
                            {
                                ViewBag.Color = "success";
                                ViewBag.Wow = "";
                                break;
                            }
                    }
                }
                else
                {
                    ViewBag.Vote = "false";
                }

                SetAccessRightsforComms();
                SetAccessRights(bookmark.UserId);

                return View(bookmark);
            }
            catch
            {
                ViewBag.ErrorMessage = "The Bookmark you are trying to access" +
                                        " doesn't seem to exist!";
                return View("Views/Shared/Error.cshtml");
            }
        }


        // Adaugarea unui comentariu asociat unui articol in bza de date(aici avem metoda New din CommentsController)

        // De ce a fost nevoie sa o mutam? Pentru ca atunci cand merge pe else ul din Controller,
        // noi vrem sa se intoarca pe Show si sa afiseze articolul cu toate comentariile aferente + mesajul de validare

        [HttpPost]
        [Authorize(Roles = "RegisteredUser,Administrator")]
        public IActionResult Show([FromForm] Comment comm)
        {
            comm.Date = DateTime.Now;

            // avem nevoie de user ul care face actiunea ca sa il unseram in baza de date
            comm.UserId = _userManager.GetUserId(User);

            if (ModelState.IsValid)
            {
                db.Comments.Add(comm);
                db.SaveChanges();
                return Redirect("/Bookmarks/Show/" + comm.BookmarkId);
            }

            else
            {
                Bookmark pin = db.Bookmarks.Include("Votes")
                                           .Include("User")
                                           .Include("Votes.User")
                                           .Include("Comments")
                                           .Include("Comments.User")
                                           .Where(pin => pin.Id == comm.BookmarkId)
                                           .First();

                SetAccessRights(pin.UserId);
                SetAccessRightsforComms();

                return View(pin);
            }

        }

        // adminul nu se joaca cu categorii =)
        [Authorize(Roles = "RegisteredUser")]
        public IActionResult AddToCategory([FromForm] BookmarkCategory bkmkcat)
        {
            if (ModelState.IsValid)
            {
                // ar trebui sa verificam daca avem deja bookmark-ul in colectie

                // adaugam asocierea dintre bookmark si categorie
                db.BookmarkCategories.Add(bkmkcat);
                db.SaveChanges();

                //ne intoarcem in show-ul bookmark-ului
                return Redirect("/Bookmarks/Show/" + bkmkcat.BookmarkId);
            }
            else
            {
                ViewBag.ErrorMessage = "We were unable to add the bookmark" +
                                        " to the category!";
                return View("Views/Shared/Error.cshtml");
            }
        }


        [Authorize(Roles = "RegisteredUser")]
        public IActionResult DeleteFromCategory(int id)
        {
            BookmarkCategory bkmkcat = (db.BookmarkCategories
                                        .Where((bkmkcat) => bkmkcat.Id == id))
                                        .First();

            int? catId = bkmkcat.CategoryId; //tin minte id-ul categoriei pt a ne intoarce in show-ul ei

            db.BookmarkCategories.Remove(bkmkcat);
            db.SaveChanges();
            return Redirect("/Categories/Show/" + catId);
        }


        [Authorize(Roles = "RegisteredUser,Administrator")]
        public IActionResult New(bool? local)
        {
            // vrem sa stim cum primim continutul media
            ViewBag.Local = true;
            if (local == false) // daca ii spunem explicit ca vr sa primim link extern
                ViewBag.Local = false;

            Bookmark bkmk = new Bookmark();
            return View(bkmk);
        }


        [Authorize(Roles = "RegisteredUser,Administrator")]
        [HttpPost]
        public async Task<IActionResult> New(Bookmark bkmk, IFormFile? BookmarkImage,[FromForm] string? MediaLink,[FromForm] string? local)
        {
            bkmk.Date = DateTime.Now;
            bkmk.NrVotes = 0;
            bkmk.UserId = _userManager.GetUserId(User); //userul curent creeaza bookmarkul

            if (BookmarkImage != null && BookmarkImage.Length > 0)
            {
                // Calea de stocare a fisierului
                var storagePath = Path.Combine(
                        _env.WebRootPath, // Preluam calea folderului wwwroot
                        "images", // Adaugam calea folderului images
                        BookmarkImage.FileName // Numele fisierului
                        );

                // Calea de afisare a fisierului care va fi stocata in baza de date
                var databaseFileName = "/images/" + BookmarkImage.FileName;

                // Uploadam fisierul la calea de storage
                using (var fileStream = new FileStream(storagePath, FileMode.Create))
                {
                    await BookmarkImage.CopyToAsync(fileStream);
                }

                //setam atributul in baza de date
                bkmk.Image = databaseFileName;
            }

            if (MediaLink != null && MediaLink.Length > 0)
                bkmk.Image = MediaLink;

            // modelstate-ul se seteaza inainte de rularea codului din controller
            // => daca modificam un attribut in controller fara de care nu se trece
            // validarea, trebuie sa o stergem mai intai si dupa sa o rerulam (?)
            if (ModelState.IsValid && bkmk.Image != null)
            {
                db.Bookmarks.Add(bkmk);
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            else
            {
                //!!
                if (bkmk.Image == null)
                    ViewBag.ImageError = "Please upload media content for your Seed!";

                if (local == "true")
                    ViewBag.Local = true;
                else
                    ViewBag.Local = false;

                return View(bkmk);
            }
        }


        [Authorize(Roles = ("RegisteredUser,Administrator"))]
        public IActionResult Edit(int? id)
        {
            try
            {
                Bookmark bkmk = db.Bookmarks.Find(id);

                // daca suntem creatorii bookmark-ului
                // adminul isi va edita doar propriile bookmark-uri

                if (bkmk.UserId == _userManager.GetUserId(User))
                {
                    return View(bkmk);
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
                ViewBag.ErrorMessage = "The Bookmark you are trying to edit" +
                                        "doesn't seem to exist!";
                return View("Views/Shared/Error.cshtml");
            }
        }

        [Authorize(Roles = "RegisteredUser, Administrator")]
        [HttpPost]
        public IActionResult Edit(int id, Bookmark requestBkmk)
        {
            Bookmark bkmk = db.Bookmarks.Find(id);

            // daca suntem creatorii bookmark-ului 
            if (bkmk.UserId == _userManager.GetUserId(User))
            {
                if (ModelState.IsValid)
                {
                    bkmk.Title = requestBkmk.Title;
                    bkmk.Description = requestBkmk.Description;
                    db.SaveChanges();
                    return RedirectToAction("Index");
                }
                else
                {
                    return View(requestBkmk);
                }
            }
            else
            {
                ViewBag.ErrorMessage = "You don't have the right to " +
                                        "edit this bookmark!";
                return View("Views/Shared/Error.cshtml");
            }
        }

        [Authorize(Roles = "RegisteredUser,Administrator")]
        [HttpPost]
        public IActionResult Delete(int? id)
        {
            try
            {
                // ii spunem explicit ca vrem sa stearga si voturile si comentariile
                // asociate bookmark-ului
                Bookmark bkmk = db.Bookmarks.Where(bookmark => bookmark.Id == id)
                                            .Include("Comments")
                                            .Include("Votes")
                                            .First();
                // daca suntem creatorii bookmark-ului / suntem admin
                if (bkmk.UserId == _userManager.GetUserId(User) || User.IsInRole("Administrator"))
                {
                    db.Bookmarks.Remove(bkmk);
                    db.SaveChanges();
                    return RedirectToAction("Index");
                }
                else
                {
                    ViewBag.ErrorMessage = "You don't have the right to " +
                                        "delete this bookmark!";
                    return View("Views/Shared/Error.cshtml");
                }
            }
            catch
            {
                ViewBag.ErrorMessage = "The bookmark you are trying to " +
                                        "delete doesn't seem to exist!";
                return View("Views/Shared/Error.cshtml");
            }
        }

        [NonAction]
        public void SetAccessRights(string userId) // primim id-ul creatorului obiectului
        {
            // vrem sa stim daca suntem admin
            // si daca suntem inregistrat

            if (User.IsInRole("Administrator"))
            {
                ViewBag.EsteAdmin = true;
                ViewBag.EsteUser = false;
            }
            else if (User.IsInRole("RegisteredUser"))
            {
                ViewBag.EsteAdmin = false;
                ViewBag.EsteUser = true;
            }
            else
            {
                ViewBag.EsteAdmin = false;
                ViewBag.EsteUser = false;
            }

            // vrem sa stim daca suntem cel care a creat obiectul

            if (_userManager.GetUserId(User) == userId)
                ViewBag.EsteCreator = true;
            else ViewBag.EsteCreator = false;

        }

        [NonAction]
        public void SetAccessRightsforComms()
        {

            ViewBag.EsteAdmin = User.IsInRole("Administrator");

            ViewBag.UserCurent = _userManager.GetUserId(User);
        }
    }
}
