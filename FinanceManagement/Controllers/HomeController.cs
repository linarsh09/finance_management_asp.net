using FinanceManagement.Data;
using FinanceManagement.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using System.Diagnostics;
using System.Security.Claims;
namespace FinanceManagement.Controllers;
public class HomeController : Controller
{
    private readonly FinanceDbContext _context;
    private readonly IMemoryCache _cache;
    public HomeController(FinanceDbContext context, IMemoryCache cache)
    {
        _context = context;
        _cache = cache;
    }

    [Authorize]
    public async Task<IActionResult> Index()
    {
        if (!int.TryParse(
            User.FindFirstValue(ClaimTypes.NameIdentifier),
            out var userId))
        {
            return Challenge();
        }

        HttpContext.Session.SetString("LastDashboardVisitUtc", DateTimeOffset.UtcNow.ToString("O"));
        var cacheKey = $"dashboard:accounts:{userId}";
        if (!_cache.TryGetValue(cacheKey, out List<AccountSummaryViewModel>? accounts))
        {
            accounts = await _context.Accounts
            .Where(a => a.UserId == userId)
            .OrderBy(a => a.Name)
            .Select(a => new AccountSummaryViewModel
            {
                Id = a.Id,
                Name = a.Name,
                CurrencyCode = a.CurrencyCode,
                Balance =
            (a.Transactions
                .Sum(t => (decimal?)(
                    t.Type == TransactionType.Income
                        ? t.Amount
                        : -t.Amount)) ?? 0m)

            + (_context.Transfers
                .Where(t => t.ToAccountId == a.Id)
                .Sum(t => (decimal?)t.Amount) ?? 0m)

            - (_context.Transfers
                .Where(t => t.FromAccountId == a.Id)
                .Sum(t => (decimal?)t.Amount) ?? 0m)
            })
            .ToListAsync();
            _cache.Set(cacheKey, accounts, TimeSpan.FromMinutes(2));
        }

        return View(accounts);
    }

    public static string DashboardCacheKey(int userId) => $"dashboard:accounts:{userId}";
    public IActionResult Privacy() => View();
    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error() => View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
}
