using System.ComponentModel.DataAnnotations;

namespace Mss.WorkForce.Code.Models
{
    [AttributeUsage(AttributeTargets.Class, AllowMultiple =false)]
    public class ValidAttribute: ValidationAttribute
    {

        protected override ValidationResult? IsValid(object? value, ValidationContext validationContext)
        {
            if (value != null && value is ICustomValidation custom)
                return custom.CustomValidation();
            return new ValidationResult("The object does not implement  Custom Validation.");
        }

    }
}
