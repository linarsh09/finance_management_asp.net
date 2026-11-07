namespace FinanceManagement.Models;

public class AccountSummaryViewModel
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string CurrencyCode { get; set; } = string.Empty;
    public decimal Balance { get; set; }
}
