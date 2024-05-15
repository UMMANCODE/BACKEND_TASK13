using MvcPustok.Models.Enums;
using System.ComponentModel.DataAnnotations;

namespace MvcPustok.Models {
	public class Order : BaseEntity {
		public string? AppUserId { get; set; }
		[MaxLength(50)]
		public string? FullName { get; set; }
		[MaxLength(100)]
		public string? Email { get; set; }
		[Required]
		[MaxLength(30)]
		public string Phone { get; set; }
		[Required]
		[MaxLength(250)]
		public string Address { get; set; }
		[MaxLength(500)]
		public string? Note { get; set; }
		public DateTime? CreatedAt { get; set; } = DateTime.UtcNow;
		public DateTime? ModifiedAt { get; set; }
		public OrderStatus Status { get; set; }
		public AppUser? AppUser { get; set; }
		public List<OrderItem> OrderItems { get; set; } = new();
	}
}
