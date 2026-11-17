using FinanceManagement.Models;
using FinanceManagement.Repositories;
using Microsoft.Extensions.Caching.Memory;

namespace FinanceManagement.Services;

public class AccountService : IAccountService
{
    private readonly IAccountRepository _repository;
    private readonly IMemoryCache _cache;

    private readonly ICurrencyRules _currencyRules;

    public AccountService(
        IAccountRepository repository,
        IMemoryCache cache,
        ICurrencyRules currencyRules)
    {
        _repository = repository;
        _cache = cache;
        _currencyRules = currencyRules;
    }

    public async Task CreateAsync(
        int userId,
        CreateAccountViewModel model)
    {
        if (!_currencyRules.IsSupported(model.CurrencyCode))
        {
            throw new ArgumentException(
                "Допустимые валюты: KZT, USD, EUR",
                nameof(model.CurrencyCode));
        }

        var account = new Account
        {
            Name = model.Name.Trim(),
            CurrencyCode = model.CurrencyCode,
            UserId = userId
        };

        await _repository.AddAsync(account);

        _cache.Remove($"dashboard:accounts:{userId}");
    }

    public async Task<List<AccountSummaryViewModel>> GetSummariesAsync(
    int userId)
    {
        var cacheKey = $"dashboard:accounts:{userId}";

        if (_cache.TryGetValue(
                cacheKey,
                out List<AccountSummaryViewModel>? accounts)
            && accounts is not null)
        {
            return accounts;
        }

        accounts = await _repository.GetSummariesAsync(userId);

        _cache.Set(cacheKey, accounts, TimeSpan.FromMinutes(2));

        return accounts;
    }
}