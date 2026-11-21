using FinanceManagement.DTO;
using FinanceManagement.Models;
using FinanceManagement.Repositories;
using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;

namespace FinanceManagement.Services;

public class UserService : IUserService
{
    private readonly IUserRepository _repository;
    private readonly IPasswordHasher<User> _hasher;

    public UserService(
        IUserRepository repository,
        IPasswordHasher<User> hasher)
    {
        _repository = repository;
        _hasher = hasher;
    }

    public async Task<UserAuthResult> RegisterAsync(CreateUserDto dto)
    {
        var validationError = Validate(dto);

        if (validationError is not null)
        {
            return validationError;
        }

        var login = dto.Login.Trim().ToLowerInvariant();
        var email = dto.Email.Trim().ToLowerInvariant();

        if (await _repository.LoginExistsAsync(login))
        {
            return new UserAuthResult(
                false,
                Error: "Этот логин уже занят.",
                Field: nameof(dto.Login));
        }

        if (await _repository.EmailExistsAsync(email))
        {
            return new UserAuthResult(
                false,
                Error: "Эта почта уже зарегистрирована.",
                Field: nameof(dto.Email));
        }

        var user = new User
        {
            Login = login,
            Email = email,
            EmailConfirmed = false
        };

        user.PasswordHash = _hasher.HashPassword(
            user, dto.Password);

        await _repository.AddAsync(user);

        return Success(user);
    }

    public async Task<UserAuthResult> LoginAsync(LoginDto dto)
    {
        var validationError = Validate(dto);

        if (validationError is not null)
        {
            return validationError;
        }

        var loginOrEmail = dto.LoginOrEmail
            .Trim()
            .ToLowerInvariant();

        var user = await _repository.FindByLoginOrEmailAsync(
            loginOrEmail);

        if (user is null)
        {
            return InvalidCredentials();
        }

        var verification = _hasher.VerifyHashedPassword(
            user,
            user.PasswordHash,
            dto.Password);

        if (verification == PasswordVerificationResult.Failed)
        {
            return InvalidCredentials();
        }

        if (!user.EmailConfirmed)
        {
            return new UserAuthResult(
                false,
                Error: "Сначала подтвердите почту кодом из письма.");
        }

        if (verification ==
            PasswordVerificationResult.SuccessRehashNeeded)
        {
            user.PasswordHash = _hasher.HashPassword(
                user, dto.Password);

            await _repository.SaveChangesAsync();
        }

        return Success(user);
    }

    private static UserAuthResult Success(User user)
    {
        return new UserAuthResult(
            true,
            new AuthenticatedUser(
                user.Id,
                user.Login,
                user.Email,
                user.SessionVersion));
    }

    private static UserAuthResult InvalidCredentials()
    {
        return new UserAuthResult(
            false,
            Error: "Неверный логин, почта или пароль.");
    }

    private static UserAuthResult? Validate(object model)
    {
        var errors = new List<ValidationResult>();

        if (Validator.TryValidateObject(
                model,
                new ValidationContext(model),
                errors,
                validateAllProperties: true))
        {
            return null;
        }

        var error = errors[0];

        return new UserAuthResult(
            false,
            Error: error.ErrorMessage ?? "Проверьте введённые данные.",
            Field: error.MemberNames.FirstOrDefault());
    }
    public async Task<bool> IsSessionValidAsync(
    int userId,
    int sessionVersion)
    {
        var user = await _repository.FindByIdAsync(userId);

        return user is not null
            && user.EmailConfirmed
            && user.SessionVersion == sessionVersion;
    }
}