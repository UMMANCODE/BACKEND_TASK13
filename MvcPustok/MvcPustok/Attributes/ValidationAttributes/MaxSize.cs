using System;
using System.ComponentModel.DataAnnotations;

namespace MvcPustok.Attributes.ValidationAttributes
{
    public class MaxSize : ValidationAttribute
    {
        private readonly int _byteSize;
        public MaxSize(int byteSize)
        {
            _byteSize = byteSize;
        }
        protected override ValidationResult? IsValid(object? value, ValidationContext validationContext)
        {
            List<IFormFile> fileList = new();

            if (value is List<IFormFile> files) {
                fileList = files;
            }
            else if (value is IFormFile file) {
                fileList.Add(file);
            }
            foreach (var file in fileList) {

                if (file != null) {
                    if (file.Length > _byteSize) {
                        double mb = _byteSize / 1024d / 1024d;
                        return new ValidationResult($"File must be less than or equal to {mb.ToString("0.##")}mb");
                    }
                }
            }

            return ValidationResult.Success;
        }
    }
}
    
	

