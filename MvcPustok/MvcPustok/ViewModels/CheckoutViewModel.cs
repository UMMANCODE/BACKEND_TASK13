using Microsoft.AspNetCore.Mvc;

namespace MvcPustok.ViewModels {
	public class CheckoutViewModel {

		public BasketViewModel Basket { get; set; }
		public OrderCreateViewModel Order { get; set; }
	}
}