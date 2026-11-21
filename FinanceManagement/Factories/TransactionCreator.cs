using FinanceManagement.Models;   //синглетон

namespace FinanceManagement.Factories;

public abstract class TransactionCreator
{
    public abstract TransactionType Type { get; }

    public Transaction Create(
        int userId,
        CreateTransactionViewModel model,
        Account account,
        Category category)
    {
        if (account.UserId != userId || category.UserId != userId)
        {
            throw new ArgumentException(
                "Счёт и категория должны принадлежать пользователю");
        }

        if (category.Type != Type)
        {
            throw new ArgumentException(
                "Тип категории не соответствует типу операции");
        }

        var transaction = CreateTransaction();

        transaction.AccountId = account.Id;
        transaction.CategoryId = category.Id;
        transaction.Amount = model.Amount;
        transaction.Date = model.Date;
        transaction.Note = model.Note?.Trim();

        return transaction;
    }

    protected abstract Transaction CreateTransaction();
}