using System.ComponentModel.DataAnnotations;

namespace SocialBookmarkingReborn.Models
{
    public class Notification
    {
        [Key]
        public int Id { get; set; }

        public string Content { get; set; }

        public DateTime Date { get; set; }

        public string? UserId { get; set; }

        public virtual ApplicationUser? ApplicationUser { get; set; }

    }
}
