using System.ComponentModel.DataAnnotations.Schema;

namespace MvcPustok.Models {
	public class OrderItem : BaseEntity {
		public int OrderId { get; set; }
		public int BookId { get; set; }
		public int Count { get; set; }
		[Column(TypeName = "money")]
		public decimal CostPrice { get; set; }
		[Column(TypeName = "money")]
		public decimal SalePrice { get; set; }
		[Column(TypeName = "decimal(18,2)")]
		public decimal DiscountPercent { get; set; }
		public Order? Order { get; set; }
		public Book? Book { get; set; }
	}
}
