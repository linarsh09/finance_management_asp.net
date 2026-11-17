using FinanceManagement.Models;
using FinanceManagement.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using Npgsql;
using System.ComponentModel.DataAnnotations;

namespace FinanceManagement.Services;

public class TransferService : ITransferService
{
    private readonly ITransferRepository _transfers;
    private readonly ITransactionRepository _transactions;
    private readonly IMemoryCache _cache;

    private readonly ICurrencyRules _currencyRules;

    public TransferService(
        ITransferRepository transfers,
        ITransactionRepository transactions,
        IMemoryCache cache,
        ICurrencyRules currencyRules)
    {
        _transfers = transfers;
        _transactions = transactions;
        _cache = cache;
        _currencyRules = currencyRules;
    }

    public Task<List<Account>> GetAccountsAsync(int userId)
    {
        return _transfers.GetAccountsAsync(userId);
    }

    public async Task<OperationResult> CreateAsync(
        int userId,
        CreateTransferViewModel model)
    {
        var errors = new List<ValidationResult>();

        if (!Validator.TryValidateObject(
                model,
                new ValidationContext(model),
                errors,
                validateAllProperties: true))
        {
            var error = errors[0];

            return new OperationResult(
                false,
                error.ErrorMessage,
                error.MemberNames.FirstOrDefault());
        }

        if (model.Date == default)
        {
            return new OperationResult(
                false, "Укажите дату", nameof(model.Date));
        }

        if (decimal.Round(model.Amount, 2) != model.Amount)
        {
            return new OperationResult(
                false,
                "Допускается не более двух знаков после запятой",
                nameof(model.Amount));
        }

        if (model.FromAccountId == model.ToAccountId)
        {
            return new OperationResult(
                false,
                "Выберите другой счёт для зачисления",
                nameof(model.ToAccountId));
        }

        return await ExecuteAsync(userId, async () =>
        {
            var from = await _transactions.GetAccountAsync(
                model.FromAccountId, userId);

            var to = await _transactions.GetAccountAsync(
                model.ToAccountId, userId);

            if (from is null)
            {
                return new OperationResult(
                    false,
                    "Выберите свой счёт списания",
                    nameof(model.FromAccountId));
            }

            if (to is null)
            {
                return new OperationResult(
                    false,
                    "Выберите свой счёт зачисления",
                    nameof(model.ToAccountId));
            }

            if (!_currencyRules.CanTransfer(
                    from.CurrencyCode,
                    to.CurrencyCode))
            {
                return new OperationResult(
                    false,
                    "Счета должны быть в одной валюте",
                    nameof(model.ToAccountId));
            }

            var balance = await _transactions.GetBalanceAsync(
                from.Id, userId);

            if (model.Amount > balance)
            {
                return new OperationResult(
                    false,
                    $"Недостаточно средств. Доступно: " +
                    $"{balance:N2} {from.CurrencyCode}",
                    nameof(model.Amount));
            }

            await _transfers.AddAsync(new Transfer
            {
                FromAccountId = from.Id,
                ToAccountId = to.Id,
                Amount = model.Amount,
                Date = model.Date,
                Note = model.Note?.Trim()
            });

            return new OperationResult(true);
        });
    }

    public Task<OperationResult> DeleteAsync(int id, int userId)
    {
        return ExecuteAsync(userId, async () =>
        {
            var transfer = await _transfers.GetByIdAsync(id, userId);

            if (transfer is null)
            {
                return new OperationResult(
                    false,
                    "Перевод не найден или недоступен");
            }

            var recipientBalance = await _transactions.GetBalanceAsync(
                transfer.ToAccountId, userId);

            if (recipientBalance - transfer.Amount < 0m)
            {
                return new OperationResult(
                    false,
                    "Нельзя удалить перевод: на счёте получателя " +
                    "недостаточно средств для его отмены.");
            }

            await _transfers.DeleteAsync(transfer);

            return new OperationResult(true);
        });
    }

    private async Task<OperationResult> ExecuteAsync(
        int userId,
        Func<Task<OperationResult>> action)
    {
        try
        {
            var result = await _transactions
                .ExecuteSerializableAsync(action);

            if (result.Success)
            {
                _cache.Remove($"dashboard:accounts:{userId}");
            }

            return result;
        }
        catch (Exception exception) when (IsConcurrencyConflict(exception))
        {
            return new OperationResult(
                false,
                "Данные изменились во время сохранения. " +
                "Повторите действие.");
        }
    }

    private static bool IsConcurrencyConflict(Exception exception)
    {
        var postgresException = exception as PostgresException
            ?? (exception as DbUpdateException)?.InnerException
                as PostgresException;

        return postgresException?.SqlState is "40001" or "40P01";
    }


}