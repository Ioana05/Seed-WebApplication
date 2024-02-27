using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SocialBookmarkingReborn.Data;
using SocialBookmarkingReborn.Models;


namespace SocialBookmarkingReborn.Controllers
{
    public class ApplicationUsersController : Controller
    {
        private readonly ApplicationDbContext db;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        private IWebHostEnvironment _env;

        public ApplicationUsersController(
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

        // vom avea ca parametru si un bool pt a alege din show ul userului
        // daca vrem sa vedem pinurile incarcate de user sau categoriile salvate
        // de acesta
        [Authorize(Roles = "RegisteredUser,Administrator")]
        public IActionResult Show(string id, bool? buttonChoice)
        {
            ApplicationUser currUser = (from user in db.ApplicationUsers.Include("Categories").Include("Bookmark")
                                        where user.Id == id
                                        select user).First();

            // default se va deschide pe categoriile salvate 
            ViewBag.ButtonChoice = true;

            // altfel, pe pinurile incarcate de acesta
            if (buttonChoice == false)
                ViewBag.ButtonChoice = false;


            /// afisare categorii doar daca au vizibilitatea true
            /// afisare buton de editare doar daca sunt eu pe profilul meu

            //vrem sa afisam categoriile user-ului conectat
            // var categories = db.Categories
            //                    .Where(cat => cat.UserId == _userManager.GetUserId(User))

            SetAccessRightsForView();
            return View(currUser);
        }


        [HttpPost]
        [Authorize(Roles = "RegisteredUser,Administrator")]
        public async Task<IActionResult> UploadProfileImage([FromForm] IFormFile? ProfileImage)
        {
            ApplicationUser user = db.ApplicationUsers
                                     .Where(u => u.Id == _userManager.GetUserId(User))
                                     .First();

            if (ProfileImage != null && ProfileImage.Length > 0)
            {
                // Calea de stocare a fisierului
                var storagePath = Path.Combine(
                        _env.WebRootPath, // Preluam calea folderului wwwroot
                        "images", // Adaugam calea folderului images
                        ProfileImage.FileName // Numele fisierului
                        );

                // Calea de afisare a fisierului care va fi stocata in baza de date
                var databaseFileName = "/images/" + ProfileImage.FileName;

                // Uploadam fisierul la calea de storage
                using (var fileStream = new FileStream(storagePath, FileMode.Create))
                {
                    await ProfileImage.CopyToAsync(fileStream);
                }

                //setam atributul in baza de date
                user.ProfileImage = databaseFileName;
            }

            db.SaveChanges();
            return Redirect("/Users/show/" + user.Id);
        }

        [NonAction]
        public void SetAccessRightsForView()
        {
            ViewBag.UserCurent = _userManager.GetUserId(User);
        }
    }
}