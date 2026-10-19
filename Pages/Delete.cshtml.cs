using ExpenseTracker.Data;
using ExpenseTracker.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace ExpenseTracker.Pages;

public class DeleteModel : PageModel
{
    private readonly IExpenseStore _store;
    private readonly ILogger<DeleteModel> _logger;

    public DeleteModel(IExpenseStore store, ILogger<DeleteModel> logger)
    {
        _store = store;
        _logger = logger;
    }

    public Expense? Expense { get; private set; }
    public string? ErrorMessage { get; private set; }

    public async Task<IActionResult> OnGetAsync(Guid id, CancellationToken cancellationToken)
    {
        Expense = await _store.GetByIdAsync(id, cancellationToken);
        return Expense is null ? NotFound() : Page();
    }

    public async Task<IActionResult> OnPostAsync(Guid id, CancellationToken cancellationToken)
    {
        try
        {
            if (!await _store.DeleteAsync(id, cancellationToken))
            {
                return NotFound();
            }

            TempData["FlashMessage"] = "Expense deleted.";
            return RedirectToPage("/Index");
        }
        catch (InvalidDataException exception)
        {
            _logger.LogError(exception, "Cannot delete expense because local data is malformed.");
            ErrorMessage = "Your local expense data is malformed, so nothing was changed. Fix App_Data/expenses.json and try again.";
            return Page();
        }
    }
}
