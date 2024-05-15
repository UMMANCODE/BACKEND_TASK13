using System;
using System.ComponentModel.DataAnnotations;

namespace MvcPustok.Models
{
	public class Genre : AuditEntity
    {
        [MaxLength(20)]
        [MinLength(3)]
        [Required]
        public string Name { get; set; }

        public List<Book>? Books { get; set; }
	}
}

