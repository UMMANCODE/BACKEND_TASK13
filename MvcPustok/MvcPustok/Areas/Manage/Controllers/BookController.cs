using System;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MvcPustok.Areas.Manage.ViewModels;
using MvcPustok.Data;
using MvcPustok.Helpers;
using MvcPustok.Models;
using Pustok.Models.Enums;

namespace MvcPustok.Areas.Manage.Controllers {
	[Area("manage")]
	[Authorize(Roles = "admin, super_admin")]
	public class BookController : Controller {
		private readonly AppDbContext _context;
		private readonly IWebHostEnvironment _env;

		public BookController(AppDbContext context, IWebHostEnvironment env) {
			_context = context;
			_env = env;
		}

		public IActionResult Index(int page = 1) {
			var query = _context.Books.Include(x => x.Author).Include(x => x.Genre)
				.Include(x => x.BookImages.Where(x => x.Status == true || x.Status == false)).Include(x => x.BookTags).ThenInclude(x => x.Tag);
			var pageData = PaginatedList<Book>.Create(query, page, 3);

			if (pageData.TotalPages < page && pageData.TotalPages >= 1) return RedirectToAction("index", new { page = pageData.TotalPages });

			return View(pageData);
		}

		public IActionResult Create() {
			ViewBag.Authors = _context.Authors.ToList();
			ViewBag.Genres = _context.Genres.ToList();
			ViewBag.Tags = _context.Tags.ToList();
			return View();
		}

		[HttpPost]
		[ValidateAntiForgeryToken]
		public IActionResult Create(Book book) {
			if (book.PosterFile is null) ModelState.AddModelError("PosterFile", "PosterFile is required");
			if (book.HoverFile is null) ModelState.AddModelError("HoverFile", "HoverFile is required");

			if (!ModelState.IsValid) {
				ViewBag.Authors = _context.Authors.ToList();
				ViewBag.Genres = _context.Genres.ToList();
				ViewBag.Tags = _context.Tags.ToList();
				return View(book);
			}
			if (!_context.Authors.Any(x => x.Id == book.AuthorId)) {
				return RedirectToAction("notfound", "error");
			}
			if (!_context.Genres.Any(x => x.Id == book.GenreId)) {
				return RedirectToAction("notfound", "error");
			}

			foreach (var tagId in book.TagIds) {
				if (!_context.Tags.Any(x => x.Id == tagId)) return RedirectToAction("notfound", "error");
				BookTag bookTag = new() {
					TagId = tagId,
				};
				book.BookTags.Add(bookTag);
			}

			BookImages poster = new() {
				Name = FileManager.Save(book.PosterFile, _env.WebRootPath, "uploads/book"),
				Status = true
			};

			BookImages hover = new() {
				Name = FileManager.Save(book.HoverFile, _env.WebRootPath, "uploads/book"),
				Status = false
			};

			List<BookImages> images = new();

			foreach (var item in book.ImageFiles) {
				images.Add(new BookImages {
					Name = FileManager.Save(item, _env.WebRootPath, "uploads/book"),
					Status = null
				});
			}
			images.Add(poster);
			images.Add(hover);
			book.BookImages = images;

			_context.Books.Add(book);
			_context.SaveChanges();
			return RedirectToAction("index");
		}

		public IActionResult Edit(int id) {
			Book book = _context.Books.Include(x => x.BookImages)
				.Include(x => x.BookTags).FirstOrDefault(x => x.Id == id);

			if (book is null) return RedirectToAction("notfound", "error");

			ViewBag.Authors = _context.Authors.ToList();
			ViewBag.Genres = _context.Genres.ToList();
			ViewBag.Tags = _context.Tags.ToList();
			book.TagIds = book.BookTags.Select(x => x.TagId).ToList();
			return View(book);
		}

		[HttpPost]
		[ValidateAntiForgeryToken]
		public IActionResult Edit(Book book) {
			Book? existBook = _context.Books.Include(x => x.BookImages).Include(x => x.BookTags).FirstOrDefault(x => x.Id == book.Id);

			if (existBook is null) return RedirectToAction("notfound", "error");

			if (book.AuthorId != existBook.AuthorId && !_context.Authors.Any(x => x.Id == book.AuthorId))
				return RedirectToAction("notfound", "error");

			if (book.GenreId != existBook.GenreId && !_context.Genres.Any(x => x.Id == book.GenreId))
				return RedirectToAction("notfound", "error");

			existBook.BookTags.RemoveAll(x => !book.TagIds.Contains(x.TagId));

			foreach (var tagId in book.TagIds.FindAll(x => !existBook.BookTags.Any(bt => bt.TagId == x))) {
				if (!_context.Tags.Any(x => x.Id == tagId)) return RedirectToAction("notfound", "error");

				BookTag bookTag = new() {
					TagId = tagId,
				};
				existBook.BookTags.Add(bookTag);
			}

			List<string> removedFileNames = new();

			List<BookImages> removedImages = existBook.BookImages.FindAll(x => x.Status == null && !book.BookImagesIds.Contains(x.Id));
			removedFileNames = removedImages.Select(x => x.Name).ToList();

			_context.BookImages.RemoveRange(removedImages);
			if (book.PosterFile is not null) {
				BookImages poster = existBook.BookImages.FirstOrDefault(x => x.Status == true);
				if (poster is not null) {
					removedFileNames.Add(poster.Name);
					poster.Name = FileManager.Save(book.PosterFile, _env.WebRootPath, "uploads/book");
				} else {
					existBook.BookImages.Add(new BookImages {
						Name = FileManager.Save(book.PosterFile, _env.WebRootPath, "uploads/book"),
						Status = true
					});
				}
			}

			if (book.HoverFile is not null) {
				BookImages hover = existBook.BookImages.FirstOrDefault(x => x.Status == false);
				if (hover is not null) {
					removedFileNames.Add(hover.Name);
					hover.Name = FileManager.Save(book.HoverFile, _env.WebRootPath, "uploads/book");
				} else {
					existBook.BookImages.Add(new BookImages {
						Name = FileManager.Save(book.HoverFile, _env.WebRootPath, "uploads/book"),
						Status = false
					});
				}
			}

			foreach (var imgFile in book.ImageFiles) {
				BookImages bookImg = new() {
					Name = FileManager.Save(imgFile, _env.WebRootPath, "uploads/book"),
					Status = null,
				};
				existBook.BookImages.Add(bookImg);
			}

			existBook.Name = book.Name;
			existBook.Desc = book.Desc;
			existBook.SalePrice = book.SalePrice;
			existBook.CostPrice = book.CostPrice;
			existBook.DiscountPercent = book.DiscountPercent;
			existBook.IsNew = book.IsNew;
			existBook.IsFeatured = book.IsFeatured;
			existBook.StockStatus = book.StockStatus;

			existBook.ModifiedAt = DateTime.UtcNow;

			_context.SaveChanges();


			foreach (var fileName in removedFileNames) {
				FileManager.Delete(_env.WebRootPath, "uploads/book", fileName);
			}

			return RedirectToAction("index");
		}
		public IActionResult Delete(int id) {
			Book book = _context.Books.Include(x => x.BookImages).FirstOrDefault(b => b.Id == id);
			if (book is null) return RedirectToAction("notfound", "error");

			foreach (var item in book.BookImages) {
				FileManager.Delete(_env.WebRootPath, "uploads/book", item.Name);
			}

			book.IsDeleted = true;

			_context.Books.Remove(book);
			_context.SaveChanges();
			return RedirectToAction("index");
		}

		public IActionResult Reviews(int id, int page = 1) {
			var query = _context.BookReviews
				.Include(x => x.AppUser)
				.Include(x => x.Book)
				.Where(x => x.BookId == id)
				.Where(x => x.Status == ReviewStatus.Pending);

			var pageData = PaginatedList<BookReview>.Create(query, page, 5);

			if (pageData.TotalPages < page && pageData.TotalPages >= 1) return RedirectToAction("index", new { page = pageData.TotalPages });

			ViewBag.BookId = id;

			return View(pageData);
		}

		public IActionResult ChangeStatus(int id, int status) {
			BookReview? review = _context.BookReviews
				.FirstOrDefault(x => x.Id == id);
			int bookId = review.BookId;
			if (review is null) return RedirectToAction("notfound", "error");
			review.Status = (ReviewStatus)status;
			_context.SaveChanges();
			return RedirectToAction("reviews", "book", new { page = 1, id = bookId });
		}
	}
}

