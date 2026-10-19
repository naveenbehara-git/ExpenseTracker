namespace ExpenseTracker.Models;

public sealed class ExpenseTrackerOptions
{
    public const string SectionName = "ExpenseTracker";
    public string CurrencyCode { get; set; } = "USD";
}
