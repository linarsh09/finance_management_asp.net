using FinanceManagement.Models;

namespace FinanceManagement.Services;

public interface IAccountService
{
    Task CreateAsync(int userId, CreateAccountViewModel model);

    Task<List<AccountSummaryViewModel>> GetSummariesAsync(int userId);
}