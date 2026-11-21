using FinanceManagement.DTO;
using FinanceManagement.Exceptions;
using FinanceManagement.Models;
using FinanceManagement.Repositories;
using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;
using System.Security.Cryptography;

namespace FinanceManagement.Services;

public class PasswordResetService : IPasswordResetService
{
    private readonly IUserRepository _users;
    private readonly IEmailSender _emailSender;
    private readonly IPasswordHasher<User> _hasher;
    private readonly ILogger<PasswordResetService> _logger;

    public PasswordResetService(
        IUserRepository users,
        IEmailSender emailSender,
        IPasswordHasher<User> hasher,
        ILogger<PasswordResetService> logger)
    {
        _users = users;
        _emailSender = emailSender;
        _hasher = hasher;
        _logger = logger;
    }

    public async Task<OperationResult> SendCodeAsync(
        ForgotPasswordDto model)
    {
        var validationError = Validate(model);

        if (validationError is not null)
        {
            return validationError;
        }

        var email = model.Email.Trim().ToLowerInvariant();

        var code = RandomNumberGenerator
            .GetInt32(0, 1_000_000)
            .ToString("D6");

        bool shouldSend;

        try
        {
            shouldSend = await _users
                .ExecuteSerializableAsync<bool>(async () =>
                {
                    var user = await _users.FindByEmailAsync(email);

                    if (user is null || !user.EmailConfirmed)
                    {
                        return false;
                    }

                    var now = DateTimeOffset.UtcNow;

                    if (user.PasswordResetLastSentAt.HasValue
                        && now - user.PasswordResetLastSentAt.Value
                            < TimeSpan.FromMinutes(1))
                    {
                        return false;
                    }

                    user.PasswordResetCodeHash =
                        _hasher.HashPassword(
                            user, "password-reset:" + code);

                    user.PasswordResetExpiresAt = now.AddMinutes(10);
                    user.PasswordResetAttempts = 0;
                    user.PasswordResetLastSentAt = now;

                    await _users.SaveChangesAsync();

                    return true;
                });
        }
        catch (UserDataConflictException)
        {
            return new OperationResult(true);
        }

        if (shouldSend)
        {
            try
            {
                await _emailSender.SendAsync(
                    email,
                    "Сброс пароля FinanceManagement",
                    $"Код для сброса пароля: {code}\n" +
                    "Он действует 10 минут.\n" +
                    "Если вы не запрашивали сброс, " +
                    "проигнорируйте письмо. Пароль не изменён.");
            }
            catch (Exception exception) when (
                exception is MailKit.Net.Smtp.SmtpCommandException
                or MailKit.Net.Smtp.SmtpProtocolException
                or MailKit.Security.AuthenticationException
                or System.IO.IOException
                or System.Net.Sockets.SocketException
                or System.Security.Authentication.AuthenticationException
                or TimeoutException)
            {
                _logger.LogWarning(
                    "Не удалось отправить письмо для сброса пароля.");
            }
        }

        // Одинаковый результат независимо от наличия пользователя.
        return new OperationResult(true);
    }

    public async Task<OperationResult> ResetAsync(
        ResetPasswordDto model)
    {
        var validationError = Validate(model);

        if (validationError is not null)
        {
            return validationError;
        }

        var email = model.Email.Trim().ToLowerInvariant();

        try
        {
            return await _users
                .ExecuteSerializableAsync<OperationResult>(async () =>
                {
                    var user = await _users.FindByEmailAsync(email);

                    if (user is null
                        || !user.EmailConfirmed
                        || string.IsNullOrEmpty(
                            user.PasswordResetCodeHash)
                        || !user.PasswordResetExpiresAt.HasValue
                        || user.PasswordResetExpiresAt.Value
                            <= DateTimeOffset.UtcNow
                        || user.PasswordResetAttempts >= 5)
                    {
                        return InvalidCode();
                    }

                    var verification = _hasher.VerifyHashedPassword(
                        user,
                        user.PasswordResetCodeHash,
                        "password-reset:" + model.Code);

                    if (verification ==
                        PasswordVerificationResult.Failed)
                    {
                        user.PasswordResetAttempts++;

                        await _users.SaveChangesAsync();

                        return InvalidCode();
                    }

                    user.PasswordHash = _hasher.HashPassword(
                        user, model.NewPassword);

                    user.PasswordResetCodeHash = null;
                    user.PasswordResetExpiresAt = null;
                    user.PasswordResetAttempts = 0;

                    user.SessionVersion++;

                    await _users.SaveChangesAsync();

                    return new OperationResult(true);
                });
        }
        catch (UserDataConflictException)
        {
            return new OperationResult(
                false,
                "Данные изменились одновременно. Повторите действие.");
        }
    }

    private static OperationResult InvalidCode()
    {
        return new OperationResult(
            false,
            "Код неверный, истёк или попытки закончились. " +
            "При необходимости запросите новый.",
            nameof(ResetPasswordDto.Code));
    }

    private static OperationResult? Validate(object model)
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

        return new OperationResult(
            false,
            error.ErrorMessage ?? "Проверьте введённые данные.",
            error.MemberNames.FirstOrDefault());
    }
}