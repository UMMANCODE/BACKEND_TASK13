using Microsoft.AspNetCore.Identity;

namespace MvcPustok.Models {
	public class AppUser : IdentityUser {
		public string FullName { get; set; }
	}
}
