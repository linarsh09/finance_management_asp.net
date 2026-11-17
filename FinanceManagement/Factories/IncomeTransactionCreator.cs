using FinanceManagement.Models;

namespace FinanceManagement.Factories;

public class IncomeTransactionCreator : TransactionCreator
{
    public override TransactionType Type => TransactionType.Income;

    protected override Transaction CreateTransaction()
    {
        return new Transaction
        {
            Type = TransactionType.Income
        };
    }
}