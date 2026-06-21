using System.ComponentModel.DataAnnotations;
using System.Reflection;

namespace EventManager.DAL.Validation
{
    public class NotEqualToAttribute : ValidationAttribute
    {
        private readonly string _comparisonProperty;

        public NotEqualToAttribute(string comparisonProperty)
        {
            _comparisonProperty = comparisonProperty;
        }

        protected override ValidationResult? IsValid(object? value, ValidationContext validationContext)
        {
            PropertyInfo? comparisonProperty = validationContext.ObjectType.GetProperty(_comparisonProperty);

            if (comparisonProperty == null)
            {
                return new ValidationResult($"Unknown property: {_comparisonProperty}");
            }

            object? comparisonValue = comparisonProperty.GetValue(validationContext.ObjectInstance);

            if (value == null || comparisonValue == null)
            {
                return ValidationResult.Success;
            }

            if (value.Equals(comparisonValue))
            {
                return new ValidationResult(ErrorMessage ?? "Values must be different.");
            }

            return ValidationResult.Success;
        }
    }
}