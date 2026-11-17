using FinanceManagement.Data;
using FinanceManagement.Models;
using Microsoft.EntityFrameworkCore;

namespace FinanceManagement.Repositories;

public class AccountRepository : IAccountRepository
{
    private readonly FinanceDbContext _context;

    public AccountRepository(FinanceDbContext context)
    {
        _context = context;
    }

    public async Task AddAsync(Account account)
    {
        _context.Accounts.Add(account);
        await _context.SaveChangesAsync();
    }

    public Task<List<AccountSummaryViewModel>> GetSummariesAsync(int userId)
    {
        return _context.Accounts
            .AsNoTracking()
            .Where(a => a.UserId == userId)
            .OrderBy(a => a.Name)
            .Select(a => new AccountSummaryViewModel
            {
                Id = a.Id,
                Name = a.Name,
                CurrencyCode = a.CurrencyCode,

                Balance =
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
            })
            .ToListAsync();
    }
}