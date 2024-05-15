using System.Net;
using System.Security.Claims;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MvcPustok.Data;
using MvcPustok.Models;
using MvcPustok.ViewModels;
using Newtonsoft.Json;
using Pustok.Models.Enums;

namespace MvcPustok.Controllers {
	public class BookController : Controller {
		private readonly AppDbContext _context;
		private readonly UserManager<AppUser> _userManager;

		public BookController(AppDbContext context, UserManager<AppUser> userManager) {
			_context = context;
			_userManager = userManager;
		}

		public IActionResult GetBookById(int id) {
			Book? book = _context.Books
				.Include(x => x.Genre)
				.Include(x => x.BookImages.Where(bi => bi.Status == true))
				.Include(x => x.BookTags).ThenInclude(bt => bt.Tag)
				.Include(x => x.BookReviews.Where(br => br.Status == ReviewStatus.Accepted))
				.FirstOrDefault(x => x.Id == id);
			return PartialView("_BookModalContentPartial", book);
		}

		public async Task<IActionResult> Detail(int id) {
			var vm = await GetBookDetailVM(id);
			if (vm.Book == null) return RedirectToAction("notfound", "error");
			return View(vm);
		}

		public IActionResult LoadMore(int id, int skip) {
			var result = new {
				reviewCount = _context.BookReviews
				.Where(x => x.Status == ReviewStatus.Accepted)
				.Count(x => x.BookId == id),
				reviews = _context.BookReviews
								.Include(x => x.AppUser)
								.Where(x => x.BookId == id && x.Status == ReviewStatus.Accepted)
								.OrderBy(x => x.CreatedAt)
								.Skip(skip).Take(2)
								.Select(x => new {
									x.Rate,
									x.Text,
									x.AppUser.FullName,
									x.CreatedAt
								}).ToList()
			};
			return Json(result);
		}

		[HttpPost]
		[ValidateAntiForgeryToken]
		public async Task<IActionResult> Review(BookReview review) {
			AppUser? user = await _userManager.GetUserAsync(User);
			if (user == null || !await _userManager.IsInRoleAsync(user, "member"))
				return RedirectToAction("login", "auth", new { returnUrl = Url.Action("detail", "book", new { id = review.BookId }) });

			if (!_context.Books.Any(x => x.Id == review.BookId && !x.IsDeleted))
				return RedirectToAction("notfound", "error");

			if (_context.BookReviews.Any(x => x.Id == review.BookId && x.AppUserId == user.Id))
				return RedirectToAction("notfound", "error");

			if (!ModelState.IsValid) {
				var vm = await GetBookDetailVM(review.BookId);
				vm.Review = review;
				return View("detail", vm);
			}

			if (review.Rate < 1 || review.Rate > 5) {
				ModelState.AddModelError("", "Rate must be between 1 and 5");
				var vm = await GetBookDetailVM(review.BookId);
				vm.Review = review;
				return View("detail", vm);
			}

			review.AppUserId = user.Id;
			review.CreatedAt = DateTime.Now;

			_context.BookReviews.Add(review);
			_context.SaveChanges();

			return RedirectToAction("detail", new { id = review.BookId });
		}

		private async Task<BookDetailViewModel> GetBookDetailVM(int bookId) {
			Book? book = _context.Books
				.Include(x => x.Genre)
				.Include(x => x.Author)
				.Include(x => x.BookImages)
				.Include(x => x.BookReviews.Where(br => br.Status == ReviewStatus.Accepted)
				.Take(2)).ThenInclude(br => br.AppUser)
				.Include(x => x.BookTags).ThenInclude(bt => bt.Tag)
				.FirstOrDefault(x => x.Id == bookId && !x.IsDeleted);

			BookDetailViewModel vm = new() {
				Book = book,
				RelatedBooks = _context.Books
								 .Include(x => x.Author)
								 .Include(x => x.BookImages
												 .Where(bi => bi.Status != null))
								 .Where(x => book != null && x.GenreId == book.GenreId)
								 .Take(5).ToList(),
				Review = new BookReview { BookId = bookId }
			};

			AppUser? user = await _userManager.GetUserAsync(User);

			vm.TotalReviewsCount = _context.BookReviews
				.Where(x => x.Status == ReviewStatus.Accepted)
				.Count(x => x.BookId == bookId);
			if (vm.TotalReviewsCount == 0) return vm;
			vm.AvgRate = (int)Math.Round(_context.BookReviews
				.Where(x => x.BookId == bookId)
				.Where(x => x.Status == ReviewStatus.Accepted)
				.Average(x => x.Rate));

			if (user == null) return vm;

			if (await _userManager.IsInRoleAsync(user, "member")
					&& !_context.BookReviews.Any(x => x.BookId == bookId
																						 && x.AppUserId == user.Id
																						 && x.Status == ReviewStatus.Accepted)) {
				vm.CanUserReview = true;
			}
			return vm;
		}

		public IActionResult AddToBasket(int bookId) {
			Book? book = _context.Books.FirstOrDefault(x => x.Id == bookId && !x.IsDeleted);

			if (book == null) return RedirectToAction("notfound", "error");

			if (User.Identity.IsAuthenticated && User.IsInRole("member")) {
				string userId = User.FindFirst(ClaimTypes.NameIdentifier).Value;

				BasketItem? basketItem = _context.BasketItems.FirstOrDefault(x => x.AppUserId == userId && x.BookId == bookId);

				if (basketItem == null) {
					basketItem = new BasketItem {
						AppUserId = userId,
						BookId = bookId,
						Count = 1
					};
					_context.BasketItems.Add(basketItem);
				}
				else {
					_context.BasketItems.Remove(basketItem);
				}
				_context.SaveChanges();
			}
			else {
				List<CookieBasketItemViewModel> cookieBasketItems = new();

				var basketItemsInCookie = Request.Cookies["Products"];

				if (basketItemsInCookie != null) {
					cookieBasketItems = JsonConvert.DeserializeObject<List<CookieBasketItemViewModel>>(basketItemsInCookie);
				}

				CookieBasketItemViewModel cookieBasketItem = cookieBasketItems.FirstOrDefault(x => x.BookId == bookId);

				if (cookieBasketItem == null) {
					cookieBasketItem = new CookieBasketItemViewModel {
						BookId = bookId,
						Count = 1
					};
					cookieBasketItems.Add(cookieBasketItem);
				}
				else {
					cookieBasketItems.Remove(cookieBasketItem);
				}

				Response.Cookies.Append("Products", JsonConvert.SerializeObject(cookieBasketItems));
			}
			return RedirectToAction("index", "home");
		}

		public IActionResult Increase(int id, int increaseBy) {
			Book? book = _context.Books.FirstOrDefault(x => x.Id == id && !x.IsDeleted);

			if (book == null) return RedirectToAction("notfound", "error");

			if (User.Identity.IsAuthenticated && User.IsInRole("member")) {
				string userId = User.FindFirst(ClaimTypes.NameIdentifier).Value;

				BasketItem? basketItem = _context.BasketItems.FirstOrDefault(x => x.AppUserId == userId && x.BookId == id);

				if (basketItem == null) {
					return RedirectToAction("notfound", "error");
				}
				else {
					basketItem.Count += increaseBy;
				}
				_context.SaveChanges();
			}
			else {
				List<CookieBasketItemViewModel> cookieBasketItems = new();

				var basketItemsInCookie = Request.Cookies["Products"];

				if (basketItemsInCookie != null) {
					cookieBasketItems = JsonConvert.DeserializeObject<List<CookieBasketItemViewModel>>(basketItemsInCookie);
				}

				CookieBasketItemViewModel cookieBasketItem = cookieBasketItems.FirstOrDefault(x => x.BookId == id);

				if (cookieBasketItem == null) {
					return RedirectToAction("notfound", "error");
				}
				else {
					cookieBasketItem.Count += increaseBy;
				}

				Response.Cookies.Append("Products", JsonConvert.SerializeObject(cookieBasketItems));
			}
			return RedirectToAction("index", "home");
		}

		public JsonResult IncreaseCountFetch(int id, int increaseBy) {
			var countOfBasketItems = 0;
			Increase(id, increaseBy);
			if (User.Identity.IsAuthenticated && User.IsInRole("member")) {
				string userId = User.FindFirst(ClaimTypes.NameIdentifier).Value;
				countOfBasketItems = _context.BasketItems
					.Where(x => x.AppUserId == userId)
					.Where(x => x.BookId == id)
					.Sum(x => x.Count);
			}
			else {
				var basketItemsInCookie = Request.Cookies["Products"];
				if (basketItemsInCookie != null) {
					List<CookieBasketItemViewModel> cookieBasketItems = JsonConvert.DeserializeObject<List<CookieBasketItemViewModel>>(basketItemsInCookie);
					countOfBasketItems = cookieBasketItems.Where(x => x.BookId == id).Sum(x => x.Count);
				}
			}
			return Json(countOfBasketItems);
		}
	}
}