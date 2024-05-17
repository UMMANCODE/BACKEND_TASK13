using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MvcPustok.Data;
using MvcPustok.Models;
using MvcPustok.ViewModels;

namespace MvcPustok.Controllers {
	[Authorize(Roles = "member")]
	public class MemberController : Controller {

		private readonly AppDbContext _context;
		private readonly UserManager<AppUser> _userManager;
		private readonly SignInManager<AppUser> _signInManager;

		public MemberController(AppDbContext context, UserManager<AppUser> userManager, SignInManager<AppUser> signInManager) {
			_userManager = userManager;
			_signInManager = signInManager;
			_context = context;
		}

		[Authorize(Roles = "member")]
		public async Task<IActionResult> Profile(string tab = "dashboard") {
			AppUser? user = await _userManager.GetUserAsync(User);

			if (user == null)
				return RedirectToAction("login", "account");

			ProfileViewModel profileVM = new() {
				ProfileEditVM = new() {
					FullName = user.FullName,
					Email = user.Email,
					UserName = user.UserName
				},
				Orders = _context.Orders.Include(x => x.OrderItems)
				.ThenInclude(oi => oi.Book)
				.OrderByDescending(x => x.CreatedAt)
				.Where(x => x.AppUserId == user.Id).ToList()
			};

			ViewBag.Tab = tab;

			return View(profileVM);
		}

		[Authorize(Roles = "member")]
		[HttpPost]
		[ValidateAntiForgeryToken]
		public async Task<IActionResult> Profile(ProfileEditViewModel profileEditVM, string tab = "profile") {
			ViewBag.Tab = tab;
			ProfileViewModel profileVM = new();
			profileVM.ProfileEditVM = profileEditVM;

			if (!ModelState.IsValid) return View(profileVM);

			AppUser? user = await _userManager.GetUserAsync(User);

			if (user == null) return RedirectToAction("login", "account");

			user.UserName = profileEditVM.UserName;
			user.Email = profileEditVM.Email;
			user.FullName = profileEditVM.FullName;

			if (profileEditVM.NewPassword != null) {
				var passwordResult = await _userManager.ChangePasswordAsync(user, profileEditVM.CurrentPassword, profileEditVM.NewPassword);

				if (!passwordResult.Succeeded) {
					foreach (var err in passwordResult.Errors)
						ModelState.AddModelError("", err.Description);

					return View(profileVM);
				}
			}


			var result = await _userManager.UpdateAsync(user);

			if (!result.Succeeded) {
				foreach (var err in result.Errors) {
					if (err.Code == "DuplicateUserName")
						ModelState.AddModelError("UserName", "UserName is already taken");
					else if (err.Code == "DuplicateEmail")
						ModelState.AddModelError("Email", "Email is already taken");
					else ModelState.AddModelError("", err.Description);
				}
				return View(profileVM);
			}

			await _signInManager.SignInAsync(user, false);

			return View(profileVM);
		}

	}
}
