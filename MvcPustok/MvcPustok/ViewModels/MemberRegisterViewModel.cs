using System.ComponentModel.DataAnnotations;

namespace MvcPustok.ViewModels {
	public class MemberRegisterViewModel {
		[MaxLength(25)]
		[MinLength(5)]
		[Required]
		public string UserName { get; set; }
		[Required]
		[EmailAddress]
		public string Email { get; set; }
		[Required]
		public string FullName { get; set; }
		[MaxLength(25)]
		[MinLength(8)]
		[Required]
		[DataType(DataType.Password)]
		public string Password { get; set; }
		[MaxLength(25)]
		[MinLength(8)]
		[Required]
		[DataType(DataType.Password)]
		[Compare(nameof(Password))]
		public string ConfirmPassword { get; set; }
	}
}
