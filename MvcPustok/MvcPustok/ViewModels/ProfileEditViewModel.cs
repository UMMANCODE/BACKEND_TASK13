using System.ComponentModel.DataAnnotations;

namespace MvcPustok.ViewModels {
	public class ProfileEditViewModel {
		[MaxLength(25)]
		[MinLength(5)]
		[Required]
		public string UserName { get; set; }
		[MaxLength(100)]
		[Required]
		[EmailAddress]
		public string Email { get; set; }
		[MaxLength(100)]
		[Required]
		public string FullName { get; set; }
		[MaxLength(25)]
		[MinLength(8)]
		[DataType(DataType.Password)]
		public string? CurrentPassword { get; set; }
		[MaxLength(25)]
		[MinLength(8)]
		[DataType(DataType.Password)]
		public string? NewPassword { get; set; }
		[MaxLength(25)]
		[MinLength(8)]
		[DataType(DataType.Password)]
		[Compare(nameof(NewPassword))]
		public string? ConfirmNewPassword { get; set; }
	}
}
