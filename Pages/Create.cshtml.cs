using ExpenseTracker.Data;
using ExpenseTracker.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace ExpenseTracker.Pages;

public class CreateModel : PageModel
{
    private readonly IExpenseStore _store;
    private readonly ILogger<CreateModel> _logger;

    public CreateModel(IExpenseStore store, ILogger<CreateModel> logger)
    {
        _store = store;
        _logger = logger;
    }

    [BindProperty]
    public Expense Input { get; set; } = NewExpense();

    public string? ErrorMessage { get; private set; }

    public void OnGet()
    {
        Input = NewExpense();
    }

    public async Task<IActionResult> OnPostAsync(CancellationToken cancellationToken)
    {
        Input.Title = (Input.Title ?? string.Empty).Trim();
        Input.Notes = string.IsNullOrWhiteSpace(Input.Notes) ? null : Input.Notes.Trim();
        if (!ModelState.IsValid)
        {
            return Page();
        }

        Input.Id = Guid.NewGuid();
        try
        {
            await _store.AddAsync(Input, cancellationToken);
            TempData["FlashMessage"] = "Expense added.";
            return RedirectToPage("/Index");
        }
        catch (InvalidDataException exception)
        {
            _logger.LogError(exception, "Cannot add expense because local data is malformed.");
            ErrorMessage = "Your local expense data is malformed, so nothing was changed. Fix App_Data/expenses.json and try again.";
            return Page();
        }
    }

    private static Expense NewExpense() => new()
    {
        Date = DateOnly.FromDateTime(DateTime.Today)
    };
}
