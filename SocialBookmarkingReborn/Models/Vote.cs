using System.ComponentModel.DataAnnotations;

namespace SocialBookmarkingReborn.Models
{
    public class Vote
    {
        [Key]
        public int Id { get; set; }

        // va fi required, dar nu cred ca in momentul in care voteaza trebuie sa aleaga reactia
        public string Name {  get; set; } //like, love, wow, haha, smart =)
        
        public int BookmarkId { get; set; }
        public string UserId { get; set; }
        
        public virtual ApplicationUser User { get; set; }
        public virtual Bookmark Bookmark { get; set; }
    }
}
