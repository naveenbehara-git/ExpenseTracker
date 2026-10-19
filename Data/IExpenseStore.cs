using ExpenseTracker.Models;

namespace ExpenseTracker.Data;

public interface IExpenseStore
{
    Task<IReadOnlyList<Expense>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<Expense?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task AddAsync(Expense expense, CancellationToken cancellationToken = default);
    Task UpdateAsync(Expense expense, CancellationToken cancellationToken = default);
    Task<bool> DeleteAsync(Guid id, CancellationToken cancellationToken = default);
}
