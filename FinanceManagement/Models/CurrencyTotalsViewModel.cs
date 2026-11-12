namespace FinanceManagement.Models;

public class CurrencyTotalsViewModel
{
    public string CurrencyCode { get; set; } = string.Empty;

    public decimal Income { get; set; }

    public decimal Expense { get; set; }

    public decimal Difference => Income - Expense;
}