using FinanceManagement.Models;

namespace FinanceManagement.Repositories;

public interface ITransferRepository
{
    Task<List<Account>> GetAccountsAsync(int userId);

    Task<Transfer?> GetByIdAsync(int id, int userId);

    Task AddAsync(Transfer transfer);

    Task DeleteAsync(Transfer transfer);
}