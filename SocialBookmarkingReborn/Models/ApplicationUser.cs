using Microsoft.AspNetCore.Identity;
using System.Diagnostics.Contracts;

namespace SocialBookmarkingReborn.Models
{

    // PASUL 1- USERI SI ROLURI
    // clasa IdentityUser din baza de date contine toate atributele
    // unui utilizator
    public class ApplicationUser: IdentityUser
    {
        // un user posteaza mai multe comentarii
        public override string? UserName { get; set; }

        public string? ChosenName { get; set; }

        public string? ProfileImage { get; set; }
        public virtual ICollection<Comment> Comments { get; set; }
       
        // un user poate adauga mai multe BookMarkuri 
       public virtual ICollection<Bookmark> Bookmark { get; set; }

        public virtual ICollection<Vote> Votes { get; set; }
        public virtual ICollection<Category> Categories { get; set; }
    }
}
