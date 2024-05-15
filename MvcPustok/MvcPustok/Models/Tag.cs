using System;
using System.ComponentModel.DataAnnotations;
using MvcPustok.Models;

namespace MvcPustok.Models {
	public class Tag : BaseEntity {
		public string Name { get; set; }
		public List<BookTag> BookTags { get; set; } = new();

	}
}
