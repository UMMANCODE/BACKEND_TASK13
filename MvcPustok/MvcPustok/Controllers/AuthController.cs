using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using MvcPustok.Models;
using MvcPustok.ViewModels;

namespace MvcPustok.Controllers {
	public class AuthController : Controller {
		private readonly UserManager<AppUser> _userManager;
		private readonly SignInManager<AppUser> _signInManager;

		public AuthController(UserManager<AppUser> userManager, SignInManager<AppUser> signInManager) {
			_userManager = userManager;
			_signInManager = signInManager;
		}

		[Authorize(Roles = "member")]
		public IActionResult Logout() {
			_signInManager.SignOutAsync();
			return RedirectToAction("index", "home");
		}

		public IActionResult Login() {
			return View();
		}

		[HttpPost]
		[ValidateAntiForgeryToken]
		public async Task<IActionResult> Login(MemberLoginViewModel loginVM, string? returnUrl) {
			if ((!ModelState.IsValid)) return View();

			AppUser? member = await _userManager.FindByNameAsync(loginVM.UserName);

			if (member == null || !await _userManager.IsInRoleAsync(member, "member")) {
				ModelState.AddModelError("", "Invalid UserName or Password!");
				return View();
			}

			var result = await _signInManager.PasswordSignInAsync(member, loginVM.Password, false, true);

			if (result.IsLockedOut) {
				ModelState.AddModelError("", "You are locked out for 1 minute!");
				return View();
			}
			else if (!result.Succeeded) {
				ModelState.AddModelError("", "Invalid UserName or Password!");
				return View();
			}

			var fullNameClaim = new Claim("Custom:FullName", member.FullName);
			var emailClaim = new Claim("Custom:Email", member.Email);

			var fullNameParts = member.FullName.Split(" ");
			var firstNameClaim = new Claim("Custom:FirstName", fullNameParts.Length > 0 ? fullNameParts[0] : "");
			var lastNameClaim = new Claim("Custom:LastName", fullNameParts.Length > 1 ? fullNameParts[1] : "");

			var claims = new List<Claim> { fullNameClaim, emailClaim, firstNameClaim, lastNameClaim };

			var addClaimsResult = await _userManager.AddClaimsAsync(member, claims);
			if (!addClaimsResult.Succeeded) {
				ModelState.AddModelError("", "Claims could not be added");
				return View();
			}

			return returnUrl != null ? Redirect(returnUrl) : RedirectToAction("index", "home");
		}

		public IActionResult Register() {
			return View();
		}

		[HttpPost]
		[ValidateAntiForgeryToken]
		public async Task<IActionResult> Register(MemberRegisterViewModel registerVM) {
			if (!ModelState.IsValid) return View();

			AppUser member = new() {
				UserName = registerVM.UserName,
				FullName = registerVM.FullName,
				Email = registerVM.Email
			};

			var result = await _userManager.CreateAsync(member, registerVM.Password);

			if (!result.Succeeded) {
				foreach (var error in result.Errors) {
					ModelState.AddModelError("", error.Description);
				}
				return View();
			}

			await _userManager.AddToRoleAsync(member, "member");

			await _signInManager.SignInAsync(member, false);

			return RedirectToAction("index", "home");
		}
	}
}
