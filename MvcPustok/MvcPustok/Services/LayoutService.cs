using Microsoft.EntityFrameworkCore;
using MvcPustok.Data;
using MvcPustok.Models;
using MvcPustok.ViewModels;
using Newtonsoft.Json;
using System.Security.Claims;

namespace MvcPustok.Services {
	public class LayoutService {
		private readonly AppDbContext _context;
		private readonly IHttpContextAccessor _httpContextAccessor;

		public LayoutService(AppDbContext context, IHttpContextAccessor httpContextAccessor) {
			_context = context;
			_httpContextAccessor = httpContextAccessor;
		}
		public List<Genre> GetGenres() {
			return _context.Genres.ToList();
		}

		public Dictionary<String, String> GetSettings() {
			return _context.Settings.ToDictionary(x => x.Key, x => x.Value);
		}

		public BasketViewModel GetBasket() {
			BasketViewModel vm = new();

			if (_httpContextAccessor.HttpContext.User.Identity.IsAuthenticated
				&& _httpContextAccessor.HttpContext.User.IsInRole("member")) {
				var userId = _httpContextAccessor.HttpContext.User.FindFirst(ClaimTypes.NameIdentifier).Value;

				var basketItems = _context.BasketItems
			 .Include(x => x.Book)
			 .ThenInclude(b => b.BookImages.Where(bi => bi.Status == true))
			 .Where(x => x.AppUserId == userId)
			 .ToList();

				vm.BasketItems = basketItems.Select(x => new BasketItemViewModel {
					BookId = x.BookId,
					BookName = x.Book.Name,
					BookPrice = x.Book.DiscountPercent > 0 ? (x.Book.SalePrice * (100 - x.Book.DiscountPercent) / 100) : x.Book.SalePrice,
					BookImage = x.Book.BookImages.FirstOrDefault(x => x.Status == true)?.Name,
					Count = x.Count
				}).ToList();

				vm.TotalPrice = vm.BasketItems.Sum(x => x.Count * x.BookPrice);
				vm.TotalCount = vm.BasketItems.Sum(x => x.Count);
			}
			else {
				var basketInCookie = _httpContextAccessor.HttpContext.Request.Cookies["Products"];

				if (basketInCookie != null) {
					List<CookieBasketItemViewModel> cookieBasketItemsVM = JsonConvert.DeserializeObject<List<CookieBasketItemViewModel>>(basketInCookie);
					foreach (var cookieBasketItem in cookieBasketItemsVM) {
						Book? book = _context.Books.Include(x => x.BookImages.Where(bi => bi.Status == true))
							.FirstOrDefault(x => x.Id == cookieBasketItem.BookId && !x.IsDeleted);

						if (book != null) {
							BasketItemViewModel basketItem = new() {
								BookId = cookieBasketItem.BookId,
								Count = cookieBasketItem.Count,
								BookName = book.Name,
								BookPrice = book.DiscountPercent > 0 ? (book.SalePrice * (100 - book.DiscountPercent) / 100) : book.SalePrice,
								BookImage = book.BookImages.FirstOrDefault(x => x.Status == true)?.Name
							};
							vm.BasketItems.Add(basketItem);
						}
					}
					vm.TotalPrice = vm.BasketItems.Sum(x => x.Count * x.BookPrice);
					vm.TotalCount = vm.BasketItems.Sum(x => x.Count);
				}
			}
			return vm;
		}
	}
}