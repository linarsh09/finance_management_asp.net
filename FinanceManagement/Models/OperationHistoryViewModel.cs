namespace FinanceManagement.Models;

public class OperationHistoryViewModel
{
    public int Id { get; set; }
    public bool IsTransfer { get; set; }
    public DateOnly Date { get; set; }

    public string TypeLabel { get; set; } = string.Empty;
    public string AccountLabel { get; set; } = string.Empty;
    public string? CategoryName { get; set; }

    public decimal Amount { get; set; }
    public string CurrencyCode { get; set; } = string.Empty;
    public string? Note { get; set; }
}