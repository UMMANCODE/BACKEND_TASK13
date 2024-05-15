using System;

namespace MvcPustok.ViewModels {
	public class BasketItemViewModel {
		public int BookId { get; set; }
		public string BookName { get; set; }
		public string BookImage { get; set; }
		public decimal BookPrice { get; set; }
		public int Count { get; set; }
	}
}