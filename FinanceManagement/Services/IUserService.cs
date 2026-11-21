using FinanceManagement.DTO;

namespace FinanceManagement.Services;

public interface IUserService
{
    Task<UserAuthResult> RegisterAsync(CreateUserDto dto);

    Task<UserAuthResult> LoginAsync(LoginDto dto);

    Task<bool> IsSessionValidAsync(int userId, int sessionVersion);
}