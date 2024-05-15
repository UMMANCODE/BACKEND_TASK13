using System;
using System.ComponentModel.DataAnnotations;

namespace MvcPustok.Models
{
	public class BookImages:BaseEntity
	{
		
		public int BookId { get; set; }

        [Required]
        [MaxLength(250)]
        public string Name { get; set; }

		public bool? Status { get; set; }

		public Book? Book { get; set; }
	}
}

