using FinanceManagement.Data;
using FinanceManagement.Models;
using Microsoft.EntityFrameworkCore;
using System.Data;
using FinanceManagement.Filters;

namespace FinanceManagement.Repositories;

public class TransactionRepository : ITransactionRepository
{
    private readonly FinanceDbContext _context;

    public TransactionRepository(FinanceDbContext context)
    {
        _context = context;
    }

    public Task<Account?> GetAccountAsync(int accountId, int userId)
    {
        return _context.Accounts
            .AsNoTracking()
            .FirstOrDefaultAsync(a =>
                a.Id == accountId && a.UserId == userId);
    }

    public Task<Category?> GetCategoryAsync(int categoryId, int userId)
    {
        return _context.Categories
            .AsNoTracking()
            .FirstOrDefaultAsync(c =>
                c.Id == categoryId && c.UserId == userId);
    }

    public async Task<decimal> GetBalanceAsync(
        int accountId,
        int userId)
    {
        var balance = await _context.Accounts
            .Where(a => a.Id == accountId && a.UserId == userId)
            .Select(a => (decimal?)(
                (a.Transactions
                    .Sum(t => (decimal?)(
                        t.Type == TransactionType.Income
                            ? t.Amount
                            : -t.Amount)) ?? 0m)

                + (_context.Transfers
                    .Where(t => t.ToAccountId == a.Id)
                    .Sum(t => (decimal?)t.Amount) ?? 0m)

                - (_context.Transfers
                    .Where(t => t.FromAccountId == a.Id)
                    .Sum(t => (decimal?)t.Amount) ?? 0m)
            ))
            .SingleOrDefaultAsync();

        return balance ?? throw new InvalidOperationException(
            "Счёт не найден или недоступен");
    }

    public async Task AddAsync(Transaction transaction)
    {
        _context.Transactions.Add(transaction);
        await _context.SaveChangesAsync();
    }

    public async Task<T> ExecuteSerializableAsync<T>(
        Func<Task<T>> action)
    {
        await using var dbTransaction =
            await _context.Database.BeginTransactionAsync(
                IsolationLevel.Serializable);

        try
        {
            var result = await action();

            await dbTransaction.CommitAsync();

            return result;
        }
        catch
        {
            await dbTransaction.RollbackAsync();
            throw;
        }
    }

    public Task<Transaction?> GetByIdAsync(int id, int userId)
    {
        return _context.Transactions
            .FirstOrDefaultAsync(t =>
                t.Id == id && t.Account.UserId == userId);
    }

    public async Task DeleteAsync(Transaction transaction)
    {
        _context.Transactions.Remove(transaction);
        await _context.SaveChangesAsync();
    }

    public Task SaveChangesAsync()
    {
        return _context.SaveChangesAsync();
    }

    public Task<List<Account>> GetAccountsAsync(int userId)
    {
        return _context.Accounts
            .AsNoTracking()
            .Where(a => a.UserId == userId)
            .OrderBy(a => a.Name)
            .ToListAsync();
    }

    public Task<List<Category>> GetCategoriesAsync(int userId)
    {
        return _context.Categories
            .AsNoTracking()
            .Where(c => c.UserId == userId)
            .OrderBy(c => c.Name)
            .ToListAsync();
    }

    public async Task<OperationHistoryPageViewModel> GetHistoryAsync(
    int userId,
    OperationHistoryFilter filter)
    {
        var transactionsQuery = _context.Transactions
            .AsNoTracking()
            .Where(t => t.Account.UserId == userId);

        var transfersQuery = _context.Transfers
            .AsNoTracking()
            .Where(t => t.FromAccount.UserId == userId
                     && t.ToAccount.UserId == userId);

        if (filter.AccountId.HasValue)
        {
            var accountId = filter.AccountId.Value;

            transactionsQuery = transactionsQuery
                .Where(t => t.AccountId == accountId);

            transfersQuery = transfersQuery
                .Where(t => t.FromAccountId == accountId
                         || t.ToAccountId == accountId);
        }

        if (filter.DateFrom.HasValue)
        {
            var dateFrom = filter.DateFrom.Value;

            transactionsQuery = transactionsQuery
                .Where(t => t.Date >= dateFrom);

            transfersQuery = transfersQuery
                .Where(t => t.Date >= dateFrom);
        }

        if (filter.DateTo.HasValue)
        {
            var dateTo = filter.DateTo.Value;

            transactionsQuery = transactionsQuery
                .Where(t => t.Date <= dateTo);

            transfersQuery = transfersQuery
                .Where(t => t.Date <= dateTo);
        }

        var totals = await transactionsQuery
            .GroupBy(t => t.Account.CurrencyCode)
            .Select(g => new CurrencyTotalsViewModel
            {
                CurrencyCode = g.Key,

                Income = g.Sum(t =>
                    t.Type == TransactionType.Income
                        ? t.Amount
                        : 0m),

                Expense = g.Sum(t =>
                    t.Type == TransactionType.Expense
                        ? t.Amount
                        : 0m)
            })
            .OrderBy(t => t.CurrencyCode)
            .ToListAsync();

        var transactions = await transactionsQuery
            .Select(t => new OperationHistoryViewModel
            {
                Id = t.Id,
                IsTransfer = false,
                Date = t.Date,
                TypeLabel = t.Type == TransactionType.Income
                    ? "Доход"
                    : "Расход",
                AccountLabel = t.Account.Name,
                CategoryName = t.Category.Name,
                Amount = t.Amount,
                CurrencyCode = t.Account.CurrencyCode,
                Note = t.Note
            })
            .ToListAsync();

        var transfers = await transfersQuery
            .Select(t => new OperationHistoryViewModel
            {
                Id = t.Id,
                IsTransfer = true,
                Date = t.Date,
                TypeLabel = "Перевод",
                AccountLabel = t.FromAccount.Name
                    + " → " + t.ToAccount.Name,
                CategoryName = null,
                Amount = t.Amount,
                CurrencyCode = t.FromAccount.CurrencyCode,
                Note = t.Note
            })
            .ToListAsync();

        return new OperationHistoryPageViewModel
        {
            Operations = transactions
                .Concat(transfers)
                .OrderByDescending(t => t.Date)
                .ThenByDescending(t => t.Id)
                .ThenBy(t => t.IsTransfer)
                .ToList(),

            Totals = totals,

            Accounts = await GetAccountsAsync(userId)
        };
    }
}