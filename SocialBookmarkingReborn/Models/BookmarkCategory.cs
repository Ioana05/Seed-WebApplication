using System.ComponentModel.DataAnnotations.Schema;

namespace SocialBookmarkingReborn.Models
{
    public class BookmarkCategory
    {
        //identificator unic auto-incrementat
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }
        
        //componentele relatiei many-to-many, ale caror id-uri vor
        //face parte din cheia primara
        public int? BookmarkId { get; set; }
        public int? CategoryId { get; set; }

        public virtual Bookmark? Bookmark { get; set; }
        public virtual Category? Category { get; set; }

    }
}
