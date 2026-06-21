using System.ComponentModel.DataAnnotations;
using System.Reflection;

namespace EventManager.DAL.Validation
{
    public class DateGreaterThanAttribute : ValidationAttribute
    {
        private readonly string _comparisonProperty;

        public DateGreaterThanAttribute(string comparisonProperty)
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

            DateTime endTime = (DateTime)value;
            DateTime startTime = (DateTime)comparisonValue;

            if (endTime <= startTime)
            {
                return new ValidationResult(ErrorMessage ?? "End time must be later than start time.");
            }

            return ValidationResult.Success;
        }
    }
}