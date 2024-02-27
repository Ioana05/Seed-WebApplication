﻿using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using SocialBookmarkingReborn.Models;

namespace SocialBookmarkingReborn.Data
{
    // PASUL 3 - USERI SI ROLURI
    public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        public DbSet<ApplicationUser> ApplicationUsers { get; set; }
        public DbSet<Bookmark> Bookmarks { get; set; }
        public DbSet<Category> Categories { get; set; }
        public DbSet<Comment> Comments { get; set; }
        public DbSet<Vote> Votes { get; set; }
        public DbSet<BookmarkCategory> BookmarkCategories { get; set; }


        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            
            //definirea cheii primare compuse
            //doar CategoryId si BookmarkId pt a nu putea adauga de 
            //doua ori acelasi bookmark intr-o categorie
            modelBuilder.Entity<BookmarkCategory>()
                        .HasKey(bc => new { bc.Id, bc.BookmarkId, bc.CategoryId});


            // definire relatii cu modelele Bookmark si Category (FK)
            modelBuilder.Entity<BookmarkCategory>()
                        .HasOne(bc => bc.Bookmark)
                        .WithMany (bc => bc.BookmarkCategories)
                        .HasForeignKey(bc => bc.BookmarkId);
            modelBuilder.Entity<BookmarkCategory>()
                        .HasOne(bc => bc.Category)
                        .WithMany(bc => bc.BookmarkCategories)
                        .HasForeignKey(bc => bc.CategoryId);
            
        }

    }
}