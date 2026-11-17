namespace FinanceManagement.Services;

public interface ICurrencyRules
{
    bool IsSupported(string? currencyCode);

    bool CanTransfer(
        string? fromCurrency,
        string? toCurrency);
}