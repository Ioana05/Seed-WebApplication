using System.ComponentModel.DataAnnotations;

namespace SocialBookmarkingReborn.Models
{
    public class Bookmark
    {
        [Key]
        public int Id { get; set; }

        [Required(ErrorMessage = "The title is mandatory")]
        public string? Title { get; set; }

        [MaxLength(3000, ErrorMessage = "The description must be at most 3000 characters long")]
        public string? Description { get; set; }

        //daca vreau videoclip?
        //[Required(ErrorMessage = "Please upload an image for the bookmark!")]
        //pot sa o fac required? ca nu-i fac binding in form...
        public string? Image { get; set; }

        public DateTime Date { get; set; }

        //vrem sa tinem un counter pt voturi sau facem join de fiecare data?
        //mi se pare mai eficient sa tinem si counter sincer
        public int NrVotes { get; set; }

        public string? UserId { get; set; }

        public virtual ApplicationUser? User { get; set; } //cel care l-a creat
        public virtual ICollection<Vote>? Votes { get; set; }
        public virtual ICollection<Comment>? Comments { get; set; }
        public virtual ICollection<BookmarkCategory>? BookmarkCategories { get; set; }

    }
}
