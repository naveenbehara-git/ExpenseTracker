using System.ComponentModel.DataAnnotations;

namespace ExpenseTracker.Models;

[AttributeUsage(AttributeTargets.Property)]
public sealed class AllowedCategoryAttribute : ValidationAttribute
{
    protected override ValidationResult? IsValid(object? value, ValidationContext validationContext)
    {
        return value is string category && CategoryOptions.All.Contains(category, StringComparer.Ordinal)
            ? ValidationResult.Success
            : new ValidationResult("Choose a category from the list.");
    }
}

[AttributeUsage(AttributeTargets.Property)]
public sealed class NotDefaultDateAttribute : ValidationAttribute
{
    protected override ValidationResult? IsValid(object? value, ValidationContext validationContext)
    {
        return value is DateOnly date && date != default
            ? ValidationResult.Success
            : new ValidationResult("Choose a date.");
    }
}

[AttributeUsage(AttributeTargets.Property)]
public sealed class AtMostTwoDecimalPlacesAttribute : ValidationAttribute
{
    protected override ValidationResult? IsValid(object? value, ValidationContext validationContext)
    {
        if (value is not decimal amount)
        {
            return ValidationResult.Success;
        }

        return decimal.Round(amount, 2) == amount
            ? ValidationResult.Success
            : new ValidationResult("Amount can have at most two decimal places.");
    }
}
