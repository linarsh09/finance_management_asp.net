using FinanceManagement.Models;
using FinanceManagement.Filters;

namespace FinanceManagement.Repositories;

public interface ITransactionRepository
{
    Task<Account?> GetAccountAsync(int accountId, int userId);

    Task<Category?> GetCategoryAsync(int categoryId, int userId);

    Task<decimal> GetBalanceAsync(int accountId, int userId);

    Task AddAsync(Transaction transaction);

    Task<T> ExecuteSerializableAsync<T>(Func<Task<T>> action);

    Task<Transaction?> GetByIdAsync(int id, int userId);

    Task DeleteAsync(Transaction transaction);

    Task SaveChangesAsync();

    Task<List<Account>> GetAccountsAsync(int userId);

    Task<List<Category>> GetCategoriesAsync(int userId);

    Task<OperationHistoryPageViewModel> GetHistoryAsync(
    int userId,
    OperationHistoryFilter filter);
}