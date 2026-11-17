using FinanceManagement.Services;
using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Options;
using System.Security.Claims;
using System.Text.Encodings.Web;
using System.Text.Json;

namespace FinanceManagement.Authentication;

public class SessionAuthenticationHandler
    : AuthenticationHandler<AuthenticationSchemeOptions>,
      IAuthenticationSignInHandler
{
    public const string SchemeName = "SessionAuth";
    private const string SessionKey = "AuthenticatedUser";

    public SessionAuthenticationHandler(
        IOptionsMonitor<AuthenticationSchemeOptions> options,
        ILoggerFactory logger,
        UrlEncoder encoder)
        : base(options, logger, encoder)
    {
    }

    protected override async Task<AuthenticateResult>
        HandleAuthenticateAsync()
    {
        await Context.Session.LoadAsync();

        var json = Context.Session.GetString(SessionKey);

        if (string.IsNullOrEmpty(json))
        {
            return AuthenticateResult.NoResult();
        }

        AuthenticatedUser? user;

        try
        {
            user = JsonSerializer.Deserialize<AuthenticatedUser>(json);
        }
        catch (JsonException)
        {
            Context.Session.Remove(SessionKey);

            return AuthenticateResult.Fail(
                "Некорректные данные сессии.");
        }

        if (user is null
            || user.Id <= 0
            || string.IsNullOrWhiteSpace(user.Login))
        {
            Context.Session.Remove(SessionKey);

            return AuthenticateResult.Fail(
                "Некорректные данные пользователя.");
        }

        var claims = new[]
        {
            new Claim(
                ClaimTypes.NameIdentifier,
                user.Id.ToString()),

            new Claim(ClaimTypes.Name, user.Login),

            new Claim(ClaimTypes.Email, user.Email)
        };

        var identity = new ClaimsIdentity(claims, SchemeName);
        var principal = new ClaimsPrincipal(identity);

        var ticket = new AuthenticationTicket(
            principal, SchemeName);

        return AuthenticateResult.Success(ticket);
    }

    public async Task SignInAsync(
        ClaimsPrincipal user,
        AuthenticationProperties? properties)
    {
        if (!int.TryParse(
                user.FindFirst(ClaimTypes.NameIdentifier)?.Value,
                out var userId)
            || userId <= 0)
        {
            throw new InvalidOperationException(
                "Не указан идентификатор пользователя.");
        }

        var login = user.FindFirst(ClaimTypes.Name)?.Value;

        if (string.IsNullOrWhiteSpace(login))
        {
            throw new InvalidOperationException(
                "Не указан логин пользователя.");
        }

        var sessionUser = new AuthenticatedUser(
            userId,
            login,
            user.FindFirst(ClaimTypes.Email)?.Value ?? "");

        await Context.Session.LoadAsync();

        Context.Session.Clear();

        Context.Session.SetString(
            SessionKey,
            JsonSerializer.Serialize(sessionUser));

        await Context.Session.CommitAsync();
    }

    public async Task SignOutAsync(
        AuthenticationProperties? properties)
    {
        await Context.Session.LoadAsync();

        Context.Session.Clear();

        await Context.Session.CommitAsync();
    }

    protected override Task HandleChallengeAsync(
        AuthenticationProperties properties)
    {
        Response.Redirect(
            $"{Request.PathBase}/User/Login");

        return Task.CompletedTask;
    }
}