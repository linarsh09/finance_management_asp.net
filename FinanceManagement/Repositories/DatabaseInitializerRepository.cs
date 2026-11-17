using FinanceManagement.Data;
using Microsoft.EntityFrameworkCore;

namespace FinanceManagement.Repositories;

public class DatabaseInitializerRepository
    : IDatabaseInitializerRepository
{
    private readonly FinanceDbContext _context;

    public DatabaseInitializerRepository(FinanceDbContext context)
    {
        _context = context;
    }

    public Task InitializeAsync()
    {
        return _context.Database.MigrateAsync();
    }
}