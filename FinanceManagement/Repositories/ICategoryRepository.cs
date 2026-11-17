using FinanceManagement.Models;

namespace FinanceManagement.Repositories;

public interface ICategoryRepository
{
    Task<List<Category>> GetByUserIdAsync(int userId);
    Task AddAsync(Category category);
}