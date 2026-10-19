using ExpenseTracker.Models;

namespace ExpenseTracker.Data;

public static class StoreExtensions
{
    public static string CurrencyCode(this ExpenseTrackerOptions options) =>
        string.IsNullOrWhiteSpace(options.CurrencyCode) ? "USD" : options.CurrencyCode.Trim().ToUpperInvariant();
}
