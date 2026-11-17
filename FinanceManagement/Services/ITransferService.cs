using FinanceManagement.Models;

namespace FinanceManagement.Services;

public interface ITransferService
{
    Task<List<Account>> GetAccountsAsync(int userId);

    Task<OperationResult> CreateAsync(
        int userId,
        CreateTransferViewModel model);

    Task<OperationResult> DeleteAsync(int id, int userId);
}