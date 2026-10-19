using System.Globalization;
using ExpenseTracker.Data;
using ExpenseTracker.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Options;

namespace ExpenseTracker.Pages;

public class IndexModel : PageModel
{
    private readonly IExpenseStore _store;
    private readonly ExpenseTrackerOptions _options;
    private readonly ILogger<IndexModel> _logger;

    public IndexModel(IExpenseStore store, IOptions<ExpenseTrackerOptions> options, ILogger<IndexModel> logger)
    {
        _store = store;
        _options = options.Value;
        _logger = logger;
    }

    [BindProperty(SupportsGet = true)]
    public string? Category { get; set; }

    [BindProperty(SupportsGet = true)]
    public string? Month { get; set; }

    public IReadOnlyList<Expense> Expenses { get; private set; } = [];
    public IReadOnlyList<string> AvailableMonths { get; private set; } = [];
    public decimal TotalAmount => Expenses.Sum(expense => expense.Amount);
    public string CurrencyCode => _options.CurrencyCode();
    public string? ErrorMessage { get; private set; }
    public string? FlashMessage => TempData["FlashMessage"] as string;

    public async Task OnGetAsync(CancellationToken cancellationToken)
    {
        try
        {
            var allExpenses = await _store.GetAllAsync(cancellationToken);
            AvailableMonths = allExpenses
                .Select(expense => expense.Date.ToString("yyyy-MM", CultureInfo.InvariantCulture))
                .Distinct(StringComparer.Ordinal)
                .OrderDescending(StringComparer.Ordinal)
                .ToList();

            var filtered = allExpenses.AsEnumerable();
            if (!string.IsNullOrWhiteSpace(Category))
            {
                if (!CategoryOptions.All.Contains(Category, StringComparer.Ordinal))
                {
                    ErrorMessage = "That category filter is not available.";
                    Category = null;
                }
                else
                {
                    filtered = filtered.Where(expense => expense.Category == Category);
                }
            }

            if (!string.IsNullOrWhiteSpace(Month))
            {
                if (!DateTime.TryParseExact(Month, "yyyy-MM", CultureInfo.InvariantCulture,
                        DateTimeStyles.None, out var selectedMonth))
                {
                    ErrorMessage = "That month filter is not valid.";
                    Month = null;
                }
                else
                {
                    filtered = filtered.Where(expense => expense.Date.Year == selectedMonth.Year &&
                                                         expense.Date.Month == selectedMonth.Month);
                }
            }

            Expenses = filtered
                .OrderByDescending(expense => expense.Date)
                .ThenBy(expense => expense.Title, StringComparer.CurrentCultureIgnoreCase)
                .ToList();
        }
        catch (InvalidDataException exception)
        {
            _logger.LogError(exception, "The expense data file could not be read safely.");
            ErrorMessage = "Your expense data could not be read because the local data file is malformed. No data was replaced. Fix or restore App_Data/expenses.json, then try again.";
        }
    }
}
