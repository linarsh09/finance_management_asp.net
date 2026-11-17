using FinanceManagement.Models;
using FinanceManagement.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;
using System.Security.Claims;

namespace FinanceManagement.Controllers;

public class HomeController : Controller
{
    private readonly IAccountService _accountService;

    public HomeController(IAccountService accountService)
    {
        _accountService = accountService;
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

        HttpContext.Session.SetString(
            "LastDashboardVisitUtc",
            DateTimeOffset.UtcNow.ToString("O"));

        var accounts = await _accountService
            .GetSummariesAsync(userId);

        return View(accounts);
    }

    public static string DashboardCacheKey(int userId)
    {
        return $"dashboard:accounts:{userId}";
    }

    public IActionResult Privacy()
    {
        return View();
    }

    [ResponseCache(
        Duration = 0,
        Location = ResponseCacheLocation.None,
        NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel
        {
            RequestId = Activity.Current?.Id
                ?? HttpContext.TraceIdentifier
        });
    }
}