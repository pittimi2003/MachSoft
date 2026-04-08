using Mss.WorkForce.Code.Models.DTO.Enums;

namespace Mss.WorkForce.Code.Models.Attributtes
{
    [AttributeUsage(AttributeTargets.Property, AllowMultiple = false)]
    public class ValidationAttributes : Attribute
    {
        public ValidationType Validation { get; set; }
        public ValidationAttributes(ValidationType validation = ValidationType.None) 
        {
            Validation = validation;
        }
    }
}
