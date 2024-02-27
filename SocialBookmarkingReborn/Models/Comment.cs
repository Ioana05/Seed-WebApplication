﻿using Microsoft.Extensions.Primitives;
using System.ComponentModel.DataAnnotations;

namespace SocialBookmarkingReborn.Models
{
    public class Comment
    {
        [Key]
        public int Id { get; set; }

        [Required(ErrorMessage = "Content is mandatory")]
        public string Content { get; set; }

        public DateTime Date { get; set; }

        // ar trebui sa incercam sa adaugam si aprecieri pt un 
        // comm? sau ne complicam prea tare

        // FK
        public int? BookmarkId { get; set; }

        // tipul userului id este string
        // tot FK

        // PASUL 6 - USERI SI ROLURI
        public string? UserId { get; set; }

        public virtual ApplicationUser? User { get; set; }

        public virtual Bookmark? Bookmark { get; set; }


    }
}