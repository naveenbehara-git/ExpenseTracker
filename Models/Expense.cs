using System.ComponentModel.DataAnnotations;

namespace ExpenseTracker.Models;

public sealed class Expense
{
    public Guid Id { get; set; }

    [Required(ErrorMessage = "A title is required.")]
    [StringLength(120, ErrorMessage = "Title must be 120 characters or fewer.")]
    public string Title { get; set; } = string.Empty;

    [Required(ErrorMessage = "Choose a category.")]
    [AllowedCategory]
    public string Category { get; set; } = string.Empty;

    [Range(typeof(decimal), "0.01", "1000000000", ErrorMessage = "Amount must be greater than 0 and no more than 1,000,000,000.")]
    [AtMostTwoDecimalPlaces]
    public decimal Amount { get; set; }

    [DataType(DataType.Date)]
    [NotDefaultDate]
    public DateOnly Date { get; set; }

    [StringLength(500, ErrorMessage = "Notes must be 500 characters or fewer.")]
    public string? Notes { get; set; }

    public Expense Clone() => new()
    {
        Id = Id,
        Title = Title,
        Category = Category,
        Amount = Amount,
        Date = Date,
        Notes = Notes
    };
}
