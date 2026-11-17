using FinanceManagement.Models;

namespace FinanceManagement.Repositories;

public interface IUserRepository
{
    Task<bool> LoginExistsAsync(string login);

    Task<bool> EmailExistsAsync(string email);

    Task<User?> FindByLoginOrEmailAsync(string loginOrEmail);

    Task AddAsync(User user);

    Task SaveChangesAsync();
}