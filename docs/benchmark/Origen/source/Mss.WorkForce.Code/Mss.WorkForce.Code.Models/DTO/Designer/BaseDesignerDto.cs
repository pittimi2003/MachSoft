using Mss.WorkForce.Code.Models.Models;
using Mss.WorkForce.Code.Models.Resources;
using System.ComponentModel.DataAnnotations;
using System.Reflection;
using System.Text.Json.Serialization;

namespace Mss.WorkForce.Code.Models.DTO
{
    public class BaseDesignerDto
    {
        public Guid? Id { get; set; }

        public string? CanvasObjectType { get; set; }

        [Required(ErrorMessageResourceName = "_MANDATORYFIELD", ErrorMessageResourceType = typeof(ValidationResources))]
        public string? Name { get; set; }

        //[MaxValue("Layout.Width", ErrorMessage = "X cannot exceed the Width of the Layout.")]
        public float X { get; set; }
        
        //[MaxValue("Layout.Height", ErrorMessage = "Y cannot exceed the Height of the Layout.")]
        public float Y { get; set; }

        //[Range(0.01, float.MaxValue, ErrorMessageResourceName = "HEIGHTMUSTBEGREATERTHAN0", ErrorMessageResourceType = typeof(ValidationResources))]
        //[MaxValue(nameof(Y), "Layout.Height", ErrorMessage = "The value must not be greater than the Layout.")]
        public float Height { get; set; }

        //[Range(0.01, float.MaxValue, ErrorMessageResourceName = "WIDTHMUSTBEGREATERTHAN0", ErrorMessageResourceType = typeof(ValidationResources))]
        //[MaxValue(nameof(X), "Layout.Width", ErrorMessage = "The value must not be greater than the Layout.")]
        public float Width { get; set; }

        public string? StatusObject { get; set; }

        [JsonIgnore]
        public Guid? LayoutId { get; set; }

        [JsonIgnore]
        public Layout? Layout { get; set; }

    }

    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Field, AllowMultiple = false)]
    public class MaxValueAttribute : ValidationAttribute
    {
        /// <summary>
        /// Name of local property to be added to current value.
        /// If not specified, direct comparison is assumed (CurrentValue <= Max).
        /// </summary>
        public string AdditionalPropertyName { get; }

        /// <summary>
        /// Path of the property that defines the maximum allowed value (Layout.Height or Layout.Width).
        /// </summary>
        public string MaxPropertyPath { get; }

        /// <summary>
        /// Builder for the “direct comparison” mode: (PropertyCurrent) <= MaxPropertyPath
        /// </summary>
        /// <param name="maxPropertyPath">String with the path to the property (Layout.Width).</param>
        public MaxValueAttribute(string maxPropertyPath)
        {
            MaxPropertyPath = maxPropertyPath;
        }

        /// <summary>
        /// Builder for the “sum” modality: (PropertyCurrent + AdditionalPropertyName) <= MaxPropertyPath
        /// </summary>
        /// <param name="additionalPropertyName">Name of the local property to be added.</param>
        /// <param name="maxPropertyPath">String with the path to the property (Layout.Height).</param>
        public MaxValueAttribute(string additionalPropertyName, string maxPropertyPath)
        {
            AdditionalPropertyName = additionalPropertyName;
            MaxPropertyPath = maxPropertyPath;
        }

        protected override ValidationResult IsValid(object value, ValidationContext validationContext)
        {
            // If the current property (value) is null, we consider it valid
            if (value == null)
                return ValidationResult.Success;

            // Attempt to convert the current property to float
            if (!float.TryParse(value.ToString(), out float currentVal))
                return new ValidationResult($"{validationContext.DisplayName} is not a valid number.");

            // Get maximum value (MaxPropertyPath)
            object maxValueObj = GetNestedPropertyValue(validationContext.ObjectInstance, MaxPropertyPath);
            if (maxValueObj == null)
                return new ValidationResult($"No value was found for {MaxPropertyPath}."); // Shown when an error occurs

            if (!float.TryParse(maxValueObj.ToString(), out float maxVal))
                return new ValidationResult($"{MaxPropertyPath} is not a valid number.");

            // If AdditionalPropertyName is null, we do direct comparison.
            if (string.IsNullOrEmpty(AdditionalPropertyName))
            {
                // Compare (currentVal <= maxVal)
                if (currentVal > maxVal)
                    return new ValidationResult(ErrorMessage ?? $"{validationContext.DisplayName} cannot be greater than {MaxPropertyPath}.");
            }
            else
            {
                // Additional local ownership is sought and added
                var objectType = validationContext.ObjectType;
                var additionalPropInfo = objectType.GetProperty(AdditionalPropertyName, BindingFlags.Public | BindingFlags.Instance);

                if (additionalPropInfo == null)
                    return new ValidationResult($"No '{AdditionalPropertyName}' found in '{objectType.Name}'.");

                var additionalValObj = additionalPropInfo.GetValue(validationContext.ObjectInstance);

                // Default value
                if (additionalValObj == null)
                    additionalValObj = 0f;

                if (!float.TryParse(additionalValObj.ToString(), out float additionalVal))
                    return new ValidationResult($"{AdditionalPropertyName} is not a valid number.");

                float sum = currentVal + additionalVal;

                // Compare (currentVal + additionalVal) <= maxVal
                if (sum > maxVal)
                {
                    return new ValidationResult(ErrorMessage ?? $"The sum of {validationContext.DisplayName} + {AdditionalPropertyName} cannot exceed {MaxPropertyPath}.");
                }
            }

            return ValidationResult.Success;
        }

        /// <summary>
        /// Allows to obtain a nested property (Layout.Height o Layout.SomeProp.Width)
        /// </summary>
        private object GetNestedPropertyValue(object obj, string propertyPath)
        {
            if (obj == null)
                return null;

            var parts = propertyPath.Split('.');
            foreach (var part in parts)
            {
                var type = obj.GetType();
                var propInfo = type.GetProperty(part, BindingFlags.Public | BindingFlags.Instance);
                if (propInfo == null)
                    return null;

                obj = propInfo.GetValue(obj);
                if (obj == null)
                    return null;
            }
            return obj;
        }
    }
}
