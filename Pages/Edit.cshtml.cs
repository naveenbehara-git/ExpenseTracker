using ExpenseTracker.Data;
using ExpenseTracker.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace ExpenseTracker.Pages;

public class EditModel : PageModel
{
    private readonly IExpenseStore _store;
    private readonly ILogger<EditModel> _logger;

    public EditModel(IExpenseStore store, ILogger<EditModel> logger)
    {
        _store = store;
        _logger = logger;
    }

    [BindProperty]
    public Expense Input { get; set; } = new();

    public string? ErrorMessage { get; private set; }

    public async Task<IActionResult> OnGetAsync(Guid id, CancellationToken cancellationToken)
    {
        var expense = await _store.GetByIdAsync(id, cancellationToken);
        if (expense is null)
        {
            return NotFound();
        }

        Input = expense;
        return Page();
    }

    public async Task<IActionResult> OnPostAsync(CancellationToken cancellationToken)
    {
        Input.Title = (Input.Title ?? string.Empty).Trim();
        Input.Notes = string.IsNullOrWhiteSpace(Input.Notes) ? null : Input.Notes.Trim();
        if (!ModelState.IsValid)
        {
            return Page();
        }

        try
        {
            await _store.UpdateAsync(Input, cancellationToken);
            TempData["FlashMessage"] = "Expense updated.";
            return RedirectToPage("/Index");
        }
        catch (KeyNotFoundException)
        {
            return NotFound();
        }
        catch (InvalidDataException exception)
        {
            _logger.LogError(exception, "Cannot update expense because local data is malformed.");
            ErrorMessage = "Your local expense data is malformed, so nothing was changed. Fix App_Data/expenses.json and try again.";
            return Page();
        }
    }
}
