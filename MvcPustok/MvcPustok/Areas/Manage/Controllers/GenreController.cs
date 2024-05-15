using System;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MvcPustok.Areas.Manage.ViewModels;
using MvcPustok.Data;
using MvcPustok.Helpers;
using MvcPustok.Models;

namespace MvcPustok.Areas.Manage.Controllers {
	[Area("manage")]
	[Authorize(Roles = "admin, super_admin")]
	public class GenreController : Controller {
		private readonly AppDbContext _context;

		public GenreController(AppDbContext context) {
			_context = context;
		}

		public IActionResult Index(int page = 1) {
			var query = _context.Genres.Include(g => g.Books).OrderByDescending(x => x.Id);
			var pageData = PaginatedList<Genre>.Create(query, page, 2);

			if (pageData.TotalPages < page && pageData.TotalPages >= 1) return RedirectToAction("index", new { page = pageData.TotalPages });

			return View(pageData);
		}
		public IActionResult Create() {
			return View();
		}
		[HttpPost]
		[ValidateAntiForgeryToken]
		public IActionResult Create(Genre genre) {
			if (!ModelState.IsValid) {
				return View(genre);
			}

			if (_context.Genres.Any(x => x.Name == genre.Name)) {
				ModelState.AddModelError("Name", "Genre already exists!");
				return View(genre);
			}

			_context.Genres.Add(genre);
			_context.SaveChanges();

			return RedirectToAction("index");
		}
		public IActionResult Edit(int id) {
			Genre genre = _context.Genres.Find(id);

			if (genre == null) return RedirectToAction("notfound", "error");

			return View(genre);
		}
		[HttpPost]
		[ValidateAntiForgeryToken]
		public IActionResult Edit(Genre genre) {
			if (!ModelState.IsValid) {
				return View(genre);
			}

			Genre existGenre = _context.Genres.Find(genre.Id);

			if (existGenre == null) return RedirectToAction("notfound", "error");

			if (_context.Genres.Any(x => x.Name == genre.Name)) {
				ModelState.AddModelError("Name", "Genre already exists!");
				return View(genre);
			}

			existGenre.Name = genre.Name;
			_context.SaveChanges();

			return RedirectToAction("index");
		}
		public IActionResult Delete(int id) {
			Genre genre = _context.Genres.FirstOrDefault(x => x.Id == id);

			if (genre is null) return RedirectToAction("notfound", "error");

			_context.Genres.Remove(genre);

			_context.SaveChanges();

			return RedirectToAction("index");
		}
	}
}

