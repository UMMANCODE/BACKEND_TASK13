using MvcPustok.Models;

namespace MvcPustok.ViewModels {
	public class BookDetailViewModel {
		public Book? Book { get; set; }
		public List<Book> RelatedBooks { get; set; }
		public int TotalReviewsCount { get; set; }
		public bool CanUserReview { get; set; }
		public BookReview Review { get; set; }
		public int AvgRate { get; set; }
	}
}
