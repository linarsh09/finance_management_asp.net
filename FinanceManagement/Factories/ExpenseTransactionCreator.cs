using FinanceManagement.Models;

namespace FinanceManagement.Factories;

public class ExpenseTransactionCreator : TransactionCreator
{
    public override TransactionType Type => TransactionType.Expense;

    protected override Transaction CreateTransaction()
    {
        return new Transaction
        {
            Type = TransactionType.Expense
        };
    }
}