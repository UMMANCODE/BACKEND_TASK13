using System;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MvcPustok.Areas.Manage.ViewModels;
using MvcPustok.Data;
using MvcPustok.Models;

namespace MvcPustok.Areas.Manage.Controllers {
	[Area("manage")]
	[Authorize(Roles = "admin, super_admin")]
	public class AuthorController : Controller {
		private readonly AppDbContext _context;

		public AuthorController(AppDbContext context) {
			_context = context;
		}
		public IActionResult Index(int page = 1) {
			var query = _context.Authors.Include(a => a.Books).OrderByDescending(x => x.Id);
			var pageData = PaginatedList<Author>.Create(query, page, 2);

			if (pageData.TotalPages < page && pageData.TotalPages >= 1) return RedirectToAction("index", new { page = pageData.TotalPages });

			return View(pageData);
		}
		public IActionResult Create() {
			return View();
		}
		[HttpPost]
		[ValidateAntiForgeryToken]
		public IActionResult Create(Author author) {
			if (!ModelState.IsValid) {
				return View(author);
			}

			if (_context.Authors.Any(x => x.Fullname == author.Fullname)) {
				ModelState.AddModelError("Fullname", "Fullname already exists!");
				return View(author);
			}

			_context.Authors.Add(author);
			_context.SaveChanges();

			return RedirectToAction("index");
		}
		public IActionResult Edit(int id) {
			Author Author = _context.Authors.Find(id);

			if (Author == null) return RedirectToAction("notfound", "error");

			return View(Author);
		}
		[HttpPost]
		[ValidateAntiForgeryToken]
		public IActionResult Edit(Author Author) {
			if (!ModelState.IsValid) {
				return View(Author);
			}

			Author existAuthor = _context.Authors.Find(Author.Id);

			if (existAuthor == null) return RedirectToAction("notfound", "error");

			if (_context.Authors.Any(x => x.Fullname == Author.Fullname)) {
				ModelState.AddModelError("Name", "Author already exists!");
				return View(Author);
			}

			existAuthor.Fullname = Author.Fullname;
			_context.SaveChanges();

			return RedirectToAction("index");
		}
		public IActionResult Delete(int id) {
			Author author = _context.Authors.FirstOrDefault(x => x.Id == id);

			if (author is null) return RedirectToAction("notfound", "error");

			_context.Authors.Remove(author);

			_context.SaveChanges();

			return RedirectToAction("index");
		}
	}
}

