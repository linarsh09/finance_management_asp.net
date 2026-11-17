namespace FinanceManagement.Services;

public sealed class CurrencyRules : ICurrencyRules
{
    public bool IsSupported(string? currencyCode)
    {
        return currencyCode is "KZT" or "USD" or "EUR";
    }

    public bool CanTransfer(
        string? fromCurrency,
        string? toCurrency)
    {
        return IsSupported(fromCurrency)
            && IsSupported(toCurrency)
            && string.Equals(
                fromCurrency,
                toCurrency,
                StringComparison.Ordinal);
    }
}