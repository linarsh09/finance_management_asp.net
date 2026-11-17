namespace FinanceManagement.Models;

public class OperationHistoryPageViewModel
{
    public List<OperationHistoryViewModel> Operations { get; set; } = [];

    public List<CurrencyTotalsViewModel> Totals { get; set; } = [];

    public List<Account> Accounts { get; set; } = [];

    public bool AccountNotFound { get; set; }

    public string? ErrorMessage { get; set; }
}