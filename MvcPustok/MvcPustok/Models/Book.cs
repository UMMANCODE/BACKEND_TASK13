using MvcPustok.Attributes.ValidationAttributes;
using MvcPustok.Models;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MvcPustok.Models {
	public class Book : AuditEntity {
		public int GenreId { get; set; }
		public int AuthorId { get; set; }
		[MaxLength(50)]
		[MinLength(4)]
		public string Name { get; set; }
		[MaxLength(500)]
		public string? Desc { get; set; }
		[Column(TypeName = "money")]
		public decimal CostPrice { get; set; }
		[Column(TypeName = "money")]
		public decimal SalePrice { get; set; }
		[Column(TypeName = "decimal(18,2)")]
		public decimal DiscountPercent { get; set; }
		public bool StockStatus { get; set; }
		public bool IsNew { get; set; }
		public bool IsFeatured { get; set; }
		public Genre? Genre { get; set; }
		public Author? Author { get; set; }
		[NotMapped]
		[MaxSize(2 * 1024 * 1024)]
		[AllowedFileTypes("image/png", "image/jpeg")]
		public IFormFile? PosterFile { get; set; }
		[NotMapped]
		[MaxSize(2 * 1024 * 1024)]
		[AllowedFileTypes("image/png", "image/jpeg")]
		public IFormFile? HoverFile { get; set; }
		[NotMapped]
		[MaxSize(2 * 1024 * 1024)]
		[AllowedFileTypes("image/png", "image/jpeg")]
		public List<IFormFile>? ImageFiles { get; set; } = new();
		public List<BookImages>? BookImages { get; set; } = new();
		public List<BookTag> BookTags { get; set; } = new();
		[NotMapped]
		public List<int>? TagIds { get; set; } = new();
		[NotMapped]
		public List<int>? BookImagesIds { get; set; } = new();
		public List<BookReview>? BookReviews { get; set; } = new();
		public List<OrderItem>? OrderItems { get; set; } = new();
	}
}