namespace ExpenseTracker.Models;

public static class CategoryOptions
{
    public static IReadOnlyList<string> All { get; } =
    [
        "Food",
        "Transport",
        "Housing",
        "Utilities",
        "Health",
        "Shopping",
        "Entertainment",
        "Travel",
        "Other"
    ];
}
