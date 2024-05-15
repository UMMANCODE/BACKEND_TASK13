using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace MvcPustok.Areas.Manage.Controllers {
	[Area("Manage")]
	[Authorize(Roles = "admin, super_admin")]
	public class AdminController : Controller {
		public IActionResult Profile() {
			return View();
		}
	}
}
