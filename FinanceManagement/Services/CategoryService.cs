using FinanceManagement.Models;
using FinanceManagement.Repositories;

namespace FinanceManagement.Services;

public class CategoryService : ICategoryService
{
    private readonly ICategoryRepository _repository;

    public CategoryService(ICategoryRepository repository)
    {
        _repository = repository;
    }

    public Task<List<Category>> GetByUserIdAsync(int userId)
    {
        return _repository.GetByUserIdAsync(userId);
    }

    public async Task CreateAsync(
        int userId,
        CreateCategoryViewModel model)
    {
        var category = new Category
        {
            Name = model.Name.Trim(),
            Type = model.Type,
            UserId = userId
        };

        await _repository.AddAsync(category);
    }
}