using MvcPustok.Models;
using Pustok.Models.Enums;

namespace MvcPustok.Models {
	public class BookReview : BaseEntity {
		public string? AppUserId { get; set; }
		public int BookId { get; set; }
		public string Text { get; set; }
		public byte Rate { get; set; }
		public DateTime CreatedAt { get; set; } = DateTime.Now;
		public ReviewStatus Status { get; set; } = ReviewStatus.Pending;
		public AppUser? AppUser { get; set; }
		public Book? Book { get; set; }

	}
}