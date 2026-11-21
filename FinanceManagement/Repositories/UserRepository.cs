using FinanceManagement.Data;
using FinanceManagement.Models;
using Microsoft.EntityFrameworkCore;
using FinanceManagement.Exceptions;
using Npgsql;
using System.Data;

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

    public Task<User?> FindByEmailAsync(string email)
    {
        return _context.Users
            .SingleOrDefaultAsync(u => u.Email == email);
    }

    public async Task<T> ExecuteSerializableAsync<T>(
    Func<Task<T>> action)
    {
        await using var transaction =
            await _context.Database.BeginTransactionAsync(
                IsolationLevel.Serializable);

        try
        {
            var result = await action();

            await transaction.CommitAsync();

            return result;
        }
        catch (Exception exception) when (
            exception is PostgresException
            {
                SqlState: "40001" or "40P01"
            }
            || exception is DbUpdateException
            {
                InnerException: PostgresException
                {
                    SqlState: "40001" or "40P01"
                }
            })
        {
            await transaction.RollbackAsync();
            _context.ChangeTracker.Clear();

            throw new UserDataConflictException(exception);
        }
        catch
        {
            await transaction.RollbackAsync();
            _context.ChangeTracker.Clear();
            throw;
        }
    }

    public Task<User?> FindByIdAsync(int id)
    {
        return _context.Users
            .AsNoTracking()
            .SingleOrDefaultAsync(u => u.Id == id);
    }
}