using FinanceManagement.Models;

namespace FinanceManagement.Services;

public interface ITransactionService
{
    Task<OperationResult> CreateAsync(
        int userId,
        CreateTransactionViewModel model);

    Task<OperationResult> DeleteAsync(int id, int userId);

    Task<OperationResult> EditAsync(
    int id,
    int userId,
    CreateTransactionViewModel model);

    Task<List<Account>> GetAccountsAsync(int userId);

    Task<List<Category>> GetCategoriesAsync(int userId);

    Task<CreateTransactionViewModel?> GetEditModelAsync(
        int id,
        int userId);

    Task<OperationHistoryPageViewModel> GetHistoryAsync(
    int userId,
    DateOnly? dateFrom,
    DateOnly? dateTo,
    int? accountId,
    bool datesAreValid);
}