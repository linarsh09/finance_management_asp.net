namespace FinanceManagement.Filters;

public sealed record OperationHistoryFilter(
    int? AccountId,
    DateOnly? DateFrom,
    DateOnly? DateTo);