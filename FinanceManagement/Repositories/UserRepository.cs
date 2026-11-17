using FinanceManagement.Data;
using FinanceManagement.Models;
using Microsoft.EntityFrameworkCore;

namespace FinanceManagement.Repositories;

public class UserRepository : IUserRepository
{
    private readonly FinanceDbContext _context;

    public UserRepository(FinanceDbContext context)
    {
        _context = context;
    }

    public Task<bool> LoginExistsAsync(string login)
    {
        return _context.Users
            .AnyAsync(u => u.Login == login);
    }

    public Task<bool> EmailExistsAsync(string email)
    {
        return _context.Users
            .AnyAsync(u => u.Email == email);
    }

    public Task<User?> FindByLoginOrEmailAsync(string loginOrEmail)
    {
        return _context.Users
            .FirstOrDefaultAsync(u =>
                u.Login == loginOrEmail
                || u.Email == loginOrEmail);
    }

    public async Task AddAsync(User user)
    {
        _context.Users.Add(user);
        await _context.SaveChangesAsync();
    }

    public Task SaveChangesAsync()
    {
        return _context.SaveChangesAsync();
    }
}