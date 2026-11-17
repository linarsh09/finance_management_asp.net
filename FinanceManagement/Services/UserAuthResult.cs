namespace FinanceManagement.Services;

public record AuthenticatedUser(
    int Id,
    string Login,
    string Email);

public record UserAuthResult(
    bool Success,
    AuthenticatedUser? User = null,
    string? Error = null,
    string? Field = null);  