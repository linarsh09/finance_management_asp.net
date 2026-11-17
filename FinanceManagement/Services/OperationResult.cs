namespace FinanceManagement.Services;

public record OperationResult(
    bool Success,
    string? Error = null,
    string? Field = null);