using System;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using MvcPustok.Models;

namespace MvcPustok.Data {
	public class AppDbContext : IdentityDbContext {
		public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }
		public DbSet<Author> Authors { get; set; }
		public DbSet<BookImages> BookImages { get; set; }
		public DbSet<Book> Books { get; set; }
		public DbSet<BookTag> BookTags { get; set; }
		public DbSet<Genre> Genres { get; set; }
		public DbSet<Slider> Sliders { get; set; }
		public DbSet<Tag> Tags { get; set; }
		public DbSet<Feature> Features { get; set; }
		public DbSet<Setting> Settings { get; set; }
		public DbSet<AppUser> AppUsers { get; set; }
		public DbSet<BookReview> BookReviews { get; set; }
		public DbSet<BasketItem> BasketItems { get; set; }
		public DbSet<Order> Orders { get; set; }
		public DbSet<OrderItem> OrderItems { get; set; }

		override protected void OnModelCreating(ModelBuilder modelBuilder) {
			base.OnModelCreating(modelBuilder);
			modelBuilder.Entity<BookTag>().HasKey(x => new { x.BookId, x.TagId });
		}
	}
}

