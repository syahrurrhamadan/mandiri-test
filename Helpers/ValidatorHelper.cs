using System;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Http;

namespace FileUploadApi.Validation
{
    public class PdfFileAttribute : ValidationAttribute
    {
        protected override ValidationResult IsValid(object value, ValidationContext validationContext)
        {
            if (value is IFormFile file)
            {
                if (!file.ContentType.Equals("application/pdf", StringComparison.OrdinalIgnoreCase)
                || (Path.GetExtension(file.FileName).Equals(".pdf", StringComparison.OrdinalIgnoreCase) == false))
                {
                    return new ValidationResult("Only PDF files are allowed.");
                }
            }
            else
            {
                return new ValidationResult("Invalid file.");
            }

            return ValidationResult.Success;
        }
    }

    public class ExcelFileAttribute : ValidationAttribute
    {
        protected override ValidationResult IsValid(object value, ValidationContext validationContext)
        {
            if (value is IFormFile file)
            {
                if (!file.ContentType.Equals("application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", StringComparison.OrdinalIgnoreCase)
                    || (!Path.GetExtension(file.FileName).Equals(".xlsx", StringComparison.OrdinalIgnoreCase)
                        && !Path.GetExtension(file.FileName).Equals(".xls", StringComparison.OrdinalIgnoreCase)))
                {
                    return new ValidationResult("Only Excel files are allowed.");
                }
            }
            else if (value is null)
            {
                return ValidationResult.Success;
            }
            else
            {
                return new ValidationResult("Invalid file.");
            }

            return ValidationResult.Success;
        }
    }

    public class PngFileAttribute : ValidationAttribute
    {
        protected override ValidationResult IsValid(object value, ValidationContext validationContext)
        {
            if (value is IFormFile file)
            {
                if (!file.ContentType.Equals("image/png", StringComparison.OrdinalIgnoreCase)
                || (Path.GetExtension(file.FileName).Equals(".png", StringComparison.OrdinalIgnoreCase) == false))
                {
                    return new ValidationResult("Only png files are allowed.");
                }
            }
            else if (value is null)
            {
                return ValidationResult.Success;
            }
            else
            {
                return new ValidationResult("Invalid file.");
            }

            return ValidationResult.Success;
        }
    }
}
