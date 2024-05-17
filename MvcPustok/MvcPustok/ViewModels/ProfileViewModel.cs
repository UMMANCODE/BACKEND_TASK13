using MvcPustok.Models;

namespace MvcPustok.ViewModels {
	public class ProfileViewModel {
		public ProfileEditViewModel ProfileEditVM { get; set; }
		public List<Order> Orders { get; set; }
	}
}
