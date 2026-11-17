using FinanceManagement.Data;
using FinanceManagement.Models;
using Microsoft.EntityFrameworkCore;

namespace FinanceManagement.Repositories;

public class CategoryRepository : ICategoryRepository
{
    private readonly FinanceDbContext _context;

    public CategoryRepository(FinanceDbContext context)
    {
        _context = context;
    }

    public Task<List<Category>> GetByUserIdAsync(int userId)
    {
        return _context.Categories
            .AsNoTracking()
            .Where(c => c.UserId == userId)
            .OrderBy(c => c.Name)
            .ToListAsync();
    }

    public async Task AddAsync(Category category)
    {
        _context.Categories.Add(category);
        await _context.SaveChangesAsync();
    }
}