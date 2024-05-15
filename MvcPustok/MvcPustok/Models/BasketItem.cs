namespace MvcPustok.Models {
	public class BasketItem : BaseEntity {
		public int BookId { get; set; }
		public string AppUserId { get; set; }
		public int Count { get; set; }
		public DateTime CreatedAt { get; set; } = DateTime.Now;
		public Book? Book { get; set; }
		public AppUser? AppUser { get; set; }
	}
}
