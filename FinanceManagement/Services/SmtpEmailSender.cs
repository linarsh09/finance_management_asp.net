using FinanceManagement.Options;
using MailKit;
using MailKit.Net.Smtp;
using MailKit.Security;
using Microsoft.Extensions.Options;
using MimeKit;

namespace FinanceManagement.Services;

public class SmtpEmailSender : IEmailSender
{
    private readonly SmtpOptions _options;

    public SmtpEmailSender(IOptions<SmtpOptions> options)
    {
        _options = options.Value;
    }

    public async Task SendAsync(
        string recipientEmail,
        string subject,
        string text,
        CancellationToken cancellationToken = default)
    {
        var message = new MimeMessage();

        message.From.Add(new MailboxAddress(
            _options.FromName,
            _options.FromEmail));

        message.To.Add(MailboxAddress.Parse(recipientEmail));

        message.Subject = subject;
        message.Body = new TextPart("plain")
        {
            Text = text
        };

        using var client = new SmtpClient();

        client.Timeout = 15000;

        await client.ConnectAsync(
            _options.Host,
            _options.Port,
            SecureSocketOptions.SslOnConnect,
            cancellationToken);

        await client.AuthenticateAsync(
            _options.UserName,
            _options.Password,
            cancellationToken);

        await client.SendAsync(message, cancellationToken);

        await client.DisconnectAsync(true, cancellationToken);
    }
}