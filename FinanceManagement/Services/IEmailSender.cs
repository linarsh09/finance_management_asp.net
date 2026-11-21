namespace FinanceManagement.Services;

public interface IEmailSender
{
    Task SendAsync(
        string recipientEmail,
        string subject,
        string text,
        CancellationToken cancellationToken = default);
}