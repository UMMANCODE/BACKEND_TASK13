using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MvcPustok.Areas.Manage.ViewModels;
using MvcPustok.Data;
using MvcPustok.Helpers;
using MvcPustok.Models;

namespace MvcPustok.Areas.Manage.Controllers {
	[Area("manage")]
	[Authorize(Roles = "admin, super_admin")]
	public class SliderController : Controller {
		private readonly AppDbContext _context;
		private readonly IWebHostEnvironment _env;

		public SliderController(AppDbContext context, IWebHostEnvironment env) {
			_context = context;
			_env = env;
		}
		public IActionResult Index(int page = 1) {
			var query = _context.Sliders.OrderByDescending(x => x.Id);
			var pageData = PaginatedList<Slider>.Create(query, page, 2);

			if (pageData.TotalPages < page && pageData.TotalPages >= 1) return RedirectToAction("index", new { page = pageData.TotalPages });

			return View(pageData);
		}

		public IActionResult Create() {
			return View();
		}
		[HttpPost]
		[ValidateAntiForgeryToken]
		public IActionResult Create(Slider slider) {
			if (slider.ImageFile == null) ModelState.AddModelError("ImageFile", "ImageFile is required!");

			if (!ModelState.IsValid) return View();

			slider.ImageName = FileManager.Save(slider.ImageFile, _env.WebRootPath, "uploads/slider");

			_context.Sliders.Add(slider);
			_context.SaveChanges();

			return RedirectToAction("index");
		}
		public IActionResult Edit(int id) {
			Slider slider = _context.Sliders.FirstOrDefault(x => x.Id == id);

			if (slider == null) return RedirectToAction("notfound", "error");

			return View(slider);
		}
		[HttpPost]
		[ValidateAntiForgeryToken]
		public IActionResult Edit(Slider slider) {
			if (!ModelState.IsValid) return View();

			Slider existSlider = _context.Sliders.Find(slider.Id);
			if (existSlider == null) return RedirectToAction("notfound", "error");

			string deletedFile = null;
			if (slider.ImageFile != null) {
				if (slider.ImageFile.Length > 2 * 1024 * 1024) {
					ModelState.AddModelError("ImageFile", "File must be less or equal than 2MB");
					return View();
				}

				if (slider.ImageFile.ContentType != "image/png" && slider.ImageFile.ContentType != "image/jpeg") {
					ModelState.AddModelError("ImageFile", "File type must be png,jpeg or jpg");
					return View();
				}

				deletedFile = existSlider.ImageName;
				existSlider.ImageName = FileManager.Save(slider.ImageFile, _env.WebRootPath, "uploads/slider");

			}
			existSlider.Title1 = slider.Title1;
			existSlider.Title2 = slider.Title2;
			existSlider.Desc = slider.Desc;
			existSlider.Order = slider.Order;
			existSlider.BtnUrl = slider.BtnUrl;
			existSlider.BtnText = slider.BtnText;

			if (deletedFile != null) {
				FileManager.Delete(_env.WebRootPath, "uploads/slider", deletedFile);
			}

			_context.SaveChanges();
			return RedirectToAction("index");
		}
		public IActionResult Delete(int id) {
			Slider slider = _context.Sliders.FirstOrDefault(m => m.Id == id);
			if (slider is null) return RedirectToAction("notfound", "error");
			string deletedFile = slider.ImageName;

			FileManager.Delete(_env.WebRootPath, "uploads/slider", deletedFile);
			_context.Sliders.Remove(slider);
			_context.SaveChanges();
			return RedirectToAction("index");
		}
	}
}
