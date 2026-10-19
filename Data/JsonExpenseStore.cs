using System.Text.Json;
using ExpenseTracker.Models;

namespace ExpenseTracker.Data;

public sealed class JsonExpenseStore : IExpenseStore, IDisposable
{
    private readonly string _dataDirectory;
    private readonly string _filePath;
    private readonly SemaphoreSlim _gate = new(1, 1);
    private readonly JsonSerializerOptions _jsonOptions = new(JsonSerializerDefaults.Web)
    {
        WriteIndented = true,
        PropertyNameCaseInsensitive = true
    };

    public JsonExpenseStore(IHostEnvironment environment)
    {
        _dataDirectory = Path.Combine(environment.ContentRootPath, "App_Data");
        _filePath = Path.Combine(_dataDirectory, "expenses.json");
        Directory.CreateDirectory(_dataDirectory);
    }

    public async Task<IReadOnlyList<Expense>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        await _gate.WaitAsync(cancellationToken);
        try
        {
            return (await ReadLockedAsync(cancellationToken)).Select(expense => expense.Clone()).ToList();
        }
        finally
        {
            _gate.Release();
        }
    }

    public async Task<Expense?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        await _gate.WaitAsync(cancellationToken);
        try
        {
            return (await ReadLockedAsync(cancellationToken)).FirstOrDefault(expense => expense.Id == id)?.Clone();
        }
        finally
        {
            _gate.Release();
        }
    }

    public async Task AddAsync(Expense expense, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(expense);
        ValidateStoredExpense(expense);
        await _gate.WaitAsync(cancellationToken);
        try
        {
            var expenses = await ReadLockedAsync(cancellationToken);
            if (expenses.Any(existing => existing.Id == expense.Id))
            {
                throw new InvalidOperationException("An expense with this identifier already exists.");
            }

            expenses.Add(expense.Clone());
            await WriteLockedAsync(expenses, cancellationToken);
        }
        finally
        {
            _gate.Release();
        }
    }

    public async Task UpdateAsync(Expense expense, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(expense);
        ValidateStoredExpense(expense);
        await _gate.WaitAsync(cancellationToken);
        try
        {
            var expenses = await ReadLockedAsync(cancellationToken);
            var index = expenses.FindIndex(existing => existing.Id == expense.Id);
            if (index < 0)
            {
                throw new KeyNotFoundException("The expense no longer exists.");
            }

            expenses[index] = expense.Clone();
            await WriteLockedAsync(expenses, cancellationToken);
        }
        finally
        {
            _gate.Release();
        }
    }

    public async Task<bool> DeleteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        await _gate.WaitAsync(cancellationToken);
        try
        {
            var expenses = await ReadLockedAsync(cancellationToken);
            var removed = expenses.RemoveAll(expense => expense.Id == id) > 0;
            if (removed)
            {
                await WriteLockedAsync(expenses, cancellationToken);
            }

            return removed;
        }
        finally
        {
            _gate.Release();
        }
    }

    private async Task<List<Expense>> ReadLockedAsync(CancellationToken cancellationToken)
    {
        if (!File.Exists(_filePath))
        {
            return [];
        }

        try
        {
            await using var stream = new FileStream(
                _filePath, FileMode.Open, FileAccess.Read, FileShare.Read, 4096, useAsync: true);
            var expenses = await JsonSerializer.DeserializeAsync<List<Expense>>(
                stream, _jsonOptions, cancellationToken);
            if (expenses is null)
            {
                throw new InvalidDataException("The expense data file contains no JSON array.");
            }

            foreach (var expense in expenses)
            {
                ValidateStoredExpense(expense);
            }

            return expenses;
        }
        catch (JsonException exception)
        {
            throw new InvalidDataException(
                "The expense data file is malformed and was not changed.", exception);
        }
        catch (FormatException exception)
        {
            throw new InvalidDataException(
                "The expense data file contains an invalid value and was not changed.", exception);
        }
    }

    private static void ValidateStoredExpense(Expense? expense)
    {
        if (expense is null || expense.Id == Guid.Empty ||
            string.IsNullOrWhiteSpace(expense.Title) || expense.Title.Length > 120 ||
            !CategoryOptions.All.Contains(expense.Category, StringComparer.Ordinal) ||
            expense.Amount <= 0 || expense.Amount > 1_000_000_000m ||
            decimal.Round(expense.Amount, 2) != expense.Amount ||
            expense.Date == default || expense.Notes?.Length > 500)
        {
            throw new InvalidDataException("The expense data file contains an invalid expense and was not changed.");
        }
    }

    private async Task WriteLockedAsync(List<Expense> expenses, CancellationToken cancellationToken)
    {
        Directory.CreateDirectory(_dataDirectory);
        var temporaryPath = Path.Combine(_dataDirectory, $"expenses.{Guid.NewGuid():N}.tmp");
        try
        {
            await using (var stream = new FileStream(
                temporaryPath, FileMode.CreateNew, FileAccess.Write, FileShare.None, 4096, useAsync: true))
            {
                await JsonSerializer.SerializeAsync(stream, expenses, _jsonOptions, cancellationToken);
                await stream.FlushAsync(cancellationToken);
            }

            File.Move(temporaryPath, _filePath, overwrite: true);
        }
        finally
        {
            if (File.Exists(temporaryPath))
            {
                File.Delete(temporaryPath);
            }
        }
    }

    public void Dispose() => _gate.Dispose();
}
