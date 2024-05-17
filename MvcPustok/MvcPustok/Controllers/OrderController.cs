using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MvcPustok.Data;
using MvcPustok.Models;
using MvcPustok.ViewModels;
using System.Security.Claims;
using System.Text.Json;

namespace MvcPustok.Controllers {
	public class OrderController : Controller {
		private readonly AppDbContext _context;
		private readonly UserManager<AppUser> _userManager;

		public OrderController(AppDbContext context, UserManager<AppUser> userManager) {
			_context = context;
			_userManager = userManager;
		}
		public IActionResult Checkout() {
			CheckoutViewModel vm = new() {
				Basket = GetBasket()
			};
			return View(vm);
		}

		[Authorize(Roles = "member")]
		[HttpPost]
		[ValidateAntiForgeryToken]
		public async Task<IActionResult> Checkout(OrderCreateViewModel orderVM) {
			if (!ModelState.IsValid) {
				CheckoutViewModel vm = new() {
					Basket = GetBasket(),
					Order = orderVM
				};
				return View(vm);
			}

			AppUser user = await _userManager.GetUserAsync(User);

			Order order = new() {
				Address = orderVM.Address,
				Phone = orderVM.Phone,
				CreatedAt = DateTime.Now,
				AppUserId = user.Id,
				Email = user.Email,
				FullName = user.FullName,
				Note = orderVM.Note,
				Status = Models.Enums.OrderStatus.Pending
			};

			order.OrderItems = _context.BasketItems.Include(x => x.Book).Where(x => x.AppUserId == user.Id).Select(x => new OrderItem {
				BookId = x.BookId,
				Count = x.Count,
				SalePrice = x.Book.SalePrice,
				DiscountPercent = x.Book.DiscountPercent,
				CostPrice = x.Book.CostPrice,
			}).ToList();

			_context.Orders.Add(order);
			_context.BasketItems.RemoveRange(_context.BasketItems.Where(x => x.AppUserId == user.Id));
			_context.SaveChanges();


			return RedirectToAction("profile", "member", new { tab = "orders" });
		}

		private BasketViewModel GetBasket() {
			BasketViewModel vm = new();

			if (User.Identity.IsAuthenticated && User.IsInRole("member")) {
				var userId = User.FindFirst(ClaimTypes.NameIdentifier).Value;

				var basketItems = _context.BasketItems
				 .Include(x => x.Book)
				 .Where(x => x.AppUserId == userId)
				 .ToList();

				vm.BasketItems = basketItems.Select(x => new BasketItemViewModel {
					BookId = x.BookId,
					BookName = x.Book.Name,
					BookPrice = x.Book.DiscountPercent > 0 ? (x.Book.SalePrice * (100 - x.Book.DiscountPercent) / 100) : x.Book.SalePrice,
					Count = x.Count
				}).ToList();

				vm.TotalPrice = vm.BasketItems.Sum(x => x.Count * x.BookPrice);
			}
			else {
				var basketItemsInCookie = Request.Cookies["Products"];
				if (basketItemsInCookie is not null) {
					List<CookieBasketItemViewModel> cookieBasketItems = JsonSerializer.Deserialize<List<CookieBasketItemViewModel>>(basketItemsInCookie);
					foreach (var cookieBasketItem in cookieBasketItems) {
						Book? book = _context.Books
							.Include(x => x.BookImages.Where(bi => bi.Status == true))
							.FirstOrDefault(x => x.Id == cookieBasketItem.BookId && !x.IsDeleted);

						if (book != null) {
							BasketItemViewModel itemVM = new() {
								BookId = cookieBasketItem.BookId,
								Count = cookieBasketItem.Count,
								BookName = book.Name,
								BookPrice = book.DiscountPercent > 0 ? (book.SalePrice * (100 - book.DiscountPercent) / 100) : book.SalePrice,
							};
							vm.BasketItems.Add(itemVM);
						}
					}
					vm.TotalPrice = vm.BasketItems.Sum(x => x.Count * x.BookPrice);
					vm.TotalCount = vm.BasketItems.Sum(x => x.Count);
				}
			}
			return vm;
		}

		[Authorize(Roles = "member")]
		public async Task<IActionResult> GetOrderItems(int orderId) {
			AppUser user = await _userManager.GetUserAsync(User);

			Order order = _context.Orders
				.Include(x => x.OrderItems)
				.ThenInclude(oi => oi.Book)
				.FirstOrDefault(x => x.Id == orderId && x.AppUserId == user.Id);

			if (order == null) return RedirectToAction("notfound", "error");

			return PartialView("_OrderModalContentPartial", order.OrderItems);
		}

		[Authorize(Roles = "member")]
		public async Task<IActionResult> Cancel(int id) {
			AppUser user = await _userManager.GetUserAsync(User);

			Order order = _context.Orders
				.FirstOrDefault(x => x.Id == id && x.AppUserId == user.Id && x.Status == Models.Enums.OrderStatus.Pending);

			if (order == null) return RedirectToAction("notfound", "error");

			order.Status = Models.Enums.OrderStatus.Cancelled;
			_context.SaveChanges();
			return RedirectToAction("profile", "member", new { tab = "orders" });
		}
	}
}