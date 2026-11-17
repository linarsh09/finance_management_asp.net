using FinanceManagement.Models;

namespace FinanceManagement.Services;

public interface ICategoryService
{
    Task<List<Category>> GetByUserIdAsync(int userId);

    Task CreateAsync(
        int userId,
        CreateCategoryViewModel model);
}