using System.ComponentModel.DataAnnotations;

namespace SocialBookmarkingReborn.Models
{
    public class Category
    {
        [Key]
        public int Id { get; set; }

        [Required(ErrorMessage = "The name of the category is mandatory")]
        public string? Name { get; set; }

        public string? Description { get; set; }

        public bool Visibility { get; set; } //vrem sa avem categorii secrete =))
        
        public string? UserId { get; set; }

        public virtual ApplicationUser? User { get; set; }
        public virtual ICollection<BookmarkCategory>? BookmarkCategories { get; set; }
    }
}
