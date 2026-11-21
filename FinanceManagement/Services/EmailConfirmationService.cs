using FinanceManagement.DTO;
using FinanceManagement.Exceptions;
using FinanceManagement.Models;
using FinanceManagement.Repositories;
using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;
using System.Security.Cryptography;

namespace FinanceManagement.Services;

public class EmailConfirmationService : IEmailConfirmationService
{
    private readonly IUserRepository _users;
    private readonly IEmailSender _emailSender;
    private readonly IPasswordHasher<User> _hasher;

    public EmailConfirmationService(
        IUserRepository users,
        IEmailSender emailSender,
        IPasswordHasher<User> hasher)
    {
        _users = users;
        _emailSender = emailSender;
        _hasher = hasher;
    }

    public async Task<OperationResult> SendCodeAsync(string email)
    {
        if (string.IsNullOrWhiteSpace(email)
            || email.Length > 256
            || !new EmailAddressAttribute().IsValid(email))
        {
            return new OperationResult(
                false, "Укажите корректную почту.");
        }

        email = email.Trim().ToLowerInvariant();

        var code = RandomNumberGenerator
            .GetInt32(0, 1_000_000)
            .ToString("D6");

        OperationResult result;

        try
        {
            result = await _users
                .ExecuteSerializableAsync<OperationResult>(async () =>
                {
                    var user = await _users.FindByEmailAsync(email);

                    if (user is null || user.EmailConfirmed)
                    {
                        return new OperationResult(
                            false,
                            "Подтверждение для этого адреса недоступно.");
                    }

                    var now = DateTimeOffset.UtcNow;

                    if (user.EmailConfirmationLastSentAt.HasValue
                        && now - user.EmailConfirmationLastSentAt.Value
                            < TimeSpan.FromMinutes(1))
                    {
                        return new OperationResult(
                            false,
                            "Повторный код можно запросить через минуту.");
                    }

                    user.EmailConfirmationCodeHash =
                        _hasher.HashPassword(
                            user, "email-confirmation:" + code);

                    user.EmailConfirmationExpiresAt =
                        now.AddMinutes(10);

                    user.EmailConfirmationAttempts = 0;
                    user.EmailConfirmationLastSentAt = now;

                    await _users.SaveChangesAsync();

                    return new OperationResult(true);
                });
        }
        catch (UserDataConflictException)
        {
            return Conflict();
        }

        if (!result.Success)
        {
            return result;
        }

        try
        {
            await _emailSender.SendAsync(
                email,
                "Подтверждение почты FinanceManagement",
                $"Ваш код: {code}\n" +
                "Он действует 10 минут.\n" +
                "Если вы не регистрировались, проигнорируйте письмо.");
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
            return new OperationResult(
                false,
                "Не удалось отправить письмо. " +
                "Попробуйте запросить новый код через минуту.");
        }

        return new OperationResult(true);
    }

    public async Task<OperationResult> ConfirmAsync(
        ConfirmEmailDto model)
    {
        var errors = new List<ValidationResult>();

        if (!Validator.TryValidateObject(
                model,
                new ValidationContext(model),
                errors,
                validateAllProperties: true))
        {
            return new OperationResult(
                false,
                errors[0].ErrorMessage,
                errors[0].MemberNames.FirstOrDefault());
        }

        var email = model.Email.Trim().ToLowerInvariant();

        try
        {
            return await _users
                .ExecuteSerializableAsync<OperationResult>(async () =>
                {
                    var user = await _users.FindByEmailAsync(email);

                    if (user is null || user.EmailConfirmed)
                    {
                        return new OperationResult(
                            false,
                            "Подтверждение недоступно. Попробуйте войти.");
                    }

                    if (string.IsNullOrEmpty(
                            user.EmailConfirmationCodeHash)
                        || !user.EmailConfirmationExpiresAt.HasValue
                        || user.EmailConfirmationExpiresAt.Value
                            <= DateTimeOffset.UtcNow)
                    {
                        return new OperationResult(
                            false,
                            "Код истёк или не был запрошен. Запросите новый.");
                    }

                    if (user.EmailConfirmationAttempts >= 5)
                    {
                        return new OperationResult(
                            false,
                            "Попытки закончились. Запросите новый код.");
                    }

                    var verification = _hasher.VerifyHashedPassword(
                        user,
                        user.EmailConfirmationCodeHash,
                        "email-confirmation:" + model.Code);

                    if (verification ==
                        PasswordVerificationResult.Failed)
                    {
                        user.EmailConfirmationAttempts++;

                        await _users.SaveChangesAsync();

                        return new OperationResult(
                            false,
                            "Неверный код.",
                            nameof(model.Code));
                    }

                    user.EmailConfirmed = true;
                    user.EmailConfirmationCodeHash = null;
                    user.EmailConfirmationExpiresAt = null;
                    user.EmailConfirmationAttempts = 0;

                    await _users.SaveChangesAsync();

                    return new OperationResult(true);
                });
        }
        catch (UserDataConflictException)
        {
            return Conflict();
        }
    }

    private static OperationResult Conflict()
    {
        return new OperationResult(
            false,
            "Данные изменились одновременно. Повторите действие.");
    }
}