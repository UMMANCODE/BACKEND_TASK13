using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using MvcPustok.Areas.Manage.ViewModels;
using MvcPustok.Models;

namespace MvcPustok.Areas.Manage.Controllers {
	[Area("Manage")]
	public class AuthController : Controller {
		private readonly UserManager<AppUser> _userManager;
		private readonly SignInManager<AppUser> _signInManager;
		private readonly RoleManager<IdentityRole> _roleManager;

		public AuthController(UserManager<AppUser> userManager, SignInManager<AppUser> signInManager, RoleManager<IdentityRole> roleManager) {
			_userManager = userManager;
			_signInManager = signInManager;
			_roleManager = roleManager;
		}

		[Authorize(Roles = "super_admin")]
		public async Task<IActionResult> CreateRoles() {
			await _roleManager.CreateAsync(new IdentityRole("super_admin"));
			await _roleManager.CreateAsync(new IdentityRole("admin"));
			await _roleManager.CreateAsync(new IdentityRole("member"));

			return Ok();
		}

		[Authorize(Roles = "super_admin")]
		public async Task<IActionResult> CreateAdmin() {
			AppUser admin = new() {
				UserName = "admin",
				FullName = "Admin",
				Email = "admin@pustok.com"
			};
			var result = await _userManager.CreateAsync(admin, "Admin123");
			if (result.Succeeded) {
				await _userManager.AddToRoleAsync(admin, "admin");
			}
			return Json(result);
		}

		public IActionResult Login() {
			return View();
		}

		[HttpPost]
		[ValidateAntiForgeryToken]
		public async Task<IActionResult> Login(AdminLoginViewModel loginVM, string returnUrl) {
			AppUser admin = await _userManager.FindByNameAsync(loginVM.UserName);

			if (admin == null || (!await _userManager.IsInRoleAsync(admin, "admin") && !await _userManager.IsInRoleAsync(admin, "super_admin"))) {
				ModelState.AddModelError("", "Incorrect Password or UserName!");
				return View();
			}


			var result = await _signInManager.PasswordSignInAsync(admin, loginVM.Password, loginVM.RememberMe, true);

			if (result.IsLockedOut) {
				ModelState.AddModelError("", "You are locked out for 1 minute!");
				return View();
			}
			else if (!result.Succeeded) {
				ModelState.AddModelError("", "Incorrect Password or UserName!");
				return View();
			}

			return returnUrl != null ? Redirect(returnUrl) : RedirectToAction("index", "dashboard");
		}

		[Authorize(Roles = "admin, super_admin")]
		public async Task<IActionResult> Logout() {
			await _signInManager.SignOutAsync();
			return RedirectToAction("login");
		}
	}
}
