using FinanceManagement.Factories;
using FinanceManagement.Models;
using FinanceManagement.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using Npgsql;
using System.ComponentModel.DataAnnotations;
using FinanceManagement.Builders;

namespace FinanceManagement.Services;

public class TransactionService : ITransactionService
{
    private readonly ITransactionRepository _repository;
    private readonly IEnumerable<TransactionCreator> _creators;
    private readonly IMemoryCache _cache;

    public TransactionService(
        ITransactionRepository repository,
        IEnumerable<TransactionCreator> creators,
        IMemoryCache cache)
    {
        _repository = repository;
        _creators = creators;
        _cache = cache;
    }

    public async Task<OperationResult> CreateAsync(
        int userId,
        CreateTransactionViewModel model)
    {
        // Проверяем атрибуты модели и при вызове вне контроллера.
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

        OperationResult result;

        try
        {
            result = await _repository
                .ExecuteSerializableAsync<OperationResult>(async () =>
                {
                    var account = await _repository.GetAccountAsync(
                        model.AccountId, userId);

                    if (account is null)
                    {
                        return new OperationResult(
                            false,
                            "Выберите свой счёт",
                            nameof(model.AccountId));
                    }

                    var category = await _repository.GetCategoryAsync(
                        model.CategoryId, userId);

                    if (category is null)
                    {
                        return new OperationResult(
                            false,
                            "Выберите свою категорию",
                            nameof(model.CategoryId));
                    }

                    var creator = _creators.SingleOrDefault(
                        c => c.Type == category.Type);

                    if (creator is null)
                    {
                        return new OperationResult(
                            false,
                            "Недопустимый тип категории",
                            nameof(model.CategoryId));
                    }

                    if (category.Type == TransactionType.Expense)
                    {
                        var balance = await _repository.GetBalanceAsync(
                            account.Id, userId);

                        if (model.Amount > balance)
                        {
                            return new OperationResult(
                                false,
                                $"Недостаточно средств. Доступно: " +
                                $"{balance:N2} {account.CurrencyCode}",
                                nameof(model.Amount));
                        }
                    }

                    var transaction = creator.Create(
                        userId, model, account, category);

                    await _repository.AddAsync(transaction);

                    return new OperationResult(true);
                });
        }
        catch (Exception exception) when (IsConcurrencyConflict(exception))
        {
            return new OperationResult(
                false,
                "Данные изменились во время сохранения. " +
                "Повторите действие.");
        }

        if (result.Success)
        {
            _cache.Remove($"dashboard:accounts:{userId}");
        }

        return result;
    }

    private static bool IsConcurrencyConflict(Exception exception)
    {
        var postgresException = exception as PostgresException
            ?? (exception as DbUpdateException)?.InnerException
                as PostgresException;

        return postgresException?.SqlState is "40001" or "40P01";
    }

    public async Task<OperationResult> DeleteAsync(int id, int userId)
    {
        OperationResult result;

        try
        {
            result = await _repository
                .ExecuteSerializableAsync<OperationResult>(async () =>
                {
                    var transaction = await _repository.GetByIdAsync(
                        id, userId);

                    if (transaction is null)
                    {
                        return new OperationResult(
                            false,
                            "Операция не найдена или недоступна");
                    }

                    var balance = await _repository.GetBalanceAsync(
                        transaction.AccountId, userId);

                    var balanceAfterDelete =
                        transaction.Type == TransactionType.Income
                            ? balance - transaction.Amount
                            : balance + transaction.Amount;

                    if (balanceAfterDelete < 0m)
                    {
                        return new OperationResult(
                            false,
                            "Нельзя удалить операцию: " +
                            "баланс счёта станет отрицательным.");
                    }

                    await _repository.DeleteAsync(transaction);

                    return new OperationResult(true);
                });
        }
        catch (Exception exception) when (IsConcurrencyConflict(exception))
        {
            return new OperationResult(
                false,
                "Данные изменились во время удаления. Повторите действие.");
        }

        if (result.Success)
        {
            _cache.Remove($"dashboard:accounts:{userId}");
        }

        return result;
    }

    public async Task<OperationResult> EditAsync(
    int id,
    int userId,
    CreateTransactionViewModel model)
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

        OperationResult result;

        try
        {
            result = await _repository
                .ExecuteSerializableAsync<OperationResult>(async () =>
                {
                    var transaction = await _repository.GetByIdAsync(
                        id, userId);

                    if (transaction is null)
                    {
                        return new OperationResult(
                            false,
                            "Операция не найдена или недоступна");
                    }

                    var account = await _repository.GetAccountAsync(
                        model.AccountId, userId);

                    if (account is null)
                    {
                        return new OperationResult(
                            false,
                            "Выберите свой счёт",
                            nameof(model.AccountId));
                    }

                    var category = await _repository.GetCategoryAsync(
                        model.CategoryId, userId);

                    if (category is null)
                    {
                        return new OperationResult(
                            false,
                            "Выберите свою категорию",
                            nameof(model.CategoryId));
                    }

                    var creator = _creators.SingleOrDefault(
                        c => c.Type == category.Type);

                    if (creator is null)
                    {
                        return new OperationResult(
                            false,
                            "Недопустимый тип категории",
                            nameof(model.CategoryId));
                    }

                    var updated = creator.Create(
                        userId, model, account, category);

                    var oldEffect =
                        transaction.Type == TransactionType.Income
                            ? transaction.Amount
                            : -transaction.Amount;

                    var newEffect =
                        updated.Type == TransactionType.Income
                            ? updated.Amount
                            : -updated.Amount;

                    var oldBalance = await _repository.GetBalanceAsync(
                        transaction.AccountId, userId);

                    if (transaction.AccountId == updated.AccountId)
                    {
                        if (oldBalance - oldEffect + newEffect < 0m)
                        {
                            return new OperationResult(
                                false,
                                "После изменения баланс станет отрицательным.");
                        }
                    }
                    else
                    {
                        var newBalance = await _repository.GetBalanceAsync(
                            updated.AccountId, userId);

                        if (oldBalance - oldEffect < 0m
                            || newBalance + newEffect < 0m)
                        {
                            return new OperationResult(
                                false,
                                "После изменения один из счетов уйдёт в минус.");
                        }
                    }

                    transaction.AccountId = updated.AccountId;
                    transaction.CategoryId = updated.CategoryId;
                    transaction.Amount = updated.Amount;
                    transaction.Date = updated.Date;
                    transaction.Type = updated.Type;
                    transaction.Note = updated.Note;

                    await _repository.SaveChangesAsync();

                    return new OperationResult(true);
                });
        }
        catch (Exception exception) when (IsConcurrencyConflict(exception))
        {
            return new OperationResult(
                false,
                "Данные изменились во время сохранения. Повторите действие.");
        }

        if (result.Success)
        {
            _cache.Remove($"dashboard:accounts:{userId}");
        }

        return result;
    }

    public Task<List<Account>> GetAccountsAsync(int userId)
    {
        return _repository.GetAccountsAsync(userId);
    }

    public Task<List<Category>> GetCategoriesAsync(int userId)
    {
        return _repository.GetCategoriesAsync(userId);
    }

    public async Task<CreateTransactionViewModel?> GetEditModelAsync(
        int id,
        int userId)
    {
        var transaction = await _repository.GetByIdAsync(id, userId);

        if (transaction is null)
        {
            return null;
        }

        return new CreateTransactionViewModel
        {
            AccountId = transaction.AccountId,
            CategoryId = transaction.CategoryId,
            Amount = transaction.Amount,
            Date = transaction.Date,
            Note = transaction.Note
        };
    }

    public async Task<OperationHistoryPageViewModel> GetHistoryAsync(
    int userId,
    DateOnly? dateFrom,
    DateOnly? dateTo,
    int? accountId,
    bool datesAreValid)
    {
        if (accountId.HasValue)
        {
            var account = await _repository.GetAccountAsync(
                accountId.Value, userId);

            if (account is null)
            {
                return new OperationHistoryPageViewModel
                {
                    AccountNotFound = true
                };
            }
        }

        string? errorMessage = null;

        if (dateFrom.HasValue && dateTo.HasValue
            && dateFrom.Value > dateTo.Value)
        {
            errorMessage =
                "Начальная дата не должна быть позже конечной";

            datesAreValid = false;
        }

        var filter = new OperationHistoryFilterBuilder()
            .ForAccount(accountId)
            .FromDate(datesAreValid ? dateFrom : null)
            .ToDate(datesAreValid ? dateTo : null)
            .Build();

        var page = await _repository.GetHistoryAsync(userId, filter);

        page.ErrorMessage = errorMessage;

        return page;
    }

}