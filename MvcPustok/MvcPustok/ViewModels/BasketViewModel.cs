namespace MvcPustok.ViewModels {
	public class BasketViewModel {
		public List<BasketItemViewModel> BasketItems { get; set; } = new();
		public decimal TotalPrice { get; set; }
		public int TotalCount { get; set; }
	}
}
