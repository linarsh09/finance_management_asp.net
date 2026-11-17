using FinanceManagement.Models;

namespace FinanceManagement.Repositories;

public interface IAccountRepository
{
    Task AddAsync(Account account);

    Task<List<AccountSummaryViewModel>> GetSummariesAsync(int userId);
}