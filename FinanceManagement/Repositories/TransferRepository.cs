using FinanceManagement.Data;
using FinanceManagement.Models;
using Microsoft.EntityFrameworkCore;

namespace FinanceManagement.Repositories;

public class TransferRepository : ITransferRepository
{
    private readonly FinanceDbContext _context;

    public TransferRepository(FinanceDbContext context)
    {
        _context = context;
    }

    public Task<List<Account>> GetAccountsAsync(int userId)
    {
        return _context.Accounts
            .AsNoTracking()
            .Where(a => a.UserId == userId)
            .OrderBy(a => a.Name)
            .ToListAsync();
    }

    public Task<Transfer?> GetByIdAsync(int id, int userId)
    {
        return _context.Transfers
            .FirstOrDefaultAsync(t =>
                t.Id == id
                && t.FromAccount.UserId == userId
                && t.ToAccount.UserId == userId);
    }

    public async Task AddAsync(Transfer transfer)
    {
        _context.Transfers.Add(transfer);
        await _context.SaveChangesAsync();
    }

    public async Task DeleteAsync(Transfer transfer)
    {
        _context.Transfers.Remove(transfer);
        await _context.SaveChangesAsync();
    }
}