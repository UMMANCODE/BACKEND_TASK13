using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MvcPustok.Data;
using MvcPustok.ViewModels;

namespace MvcPustok.Controllers;

public class HomeController : Controller {

	private AppDbContext _context;

	public HomeController(AppDbContext context) {
		_context = context;
	}
	public IActionResult Index() {
		HomeViewModel hv = new() {
			FeaturedBooks = _context.Books.Include(x => x.Author).Include(x => x.BookImages.Where(x => x.Status != null)).Where(x => x.IsFeatured).Take(10).ToList(),
			NewBooks = _context.Books.Include(x => x.Author).Include(x => x.BookImages.Where(bi => bi.Status != null)).Where(x => x.IsNew).Take(10).ToList(),
			DiscountedBooks = _context.Books.Include(x => x.Author).Include(x => x.BookImages.Where(bi => bi.Status != null)).Where(x => x.DiscountPercent > 0).OrderByDescending(x => x.DiscountPercent).Take(10).ToList(),
			Sliders = _context.Sliders.OrderBy(x => x.Order).ToList(),
			Features = _context.Features.Take(4).ToList()
		};
		return View(hv);
	}
}

