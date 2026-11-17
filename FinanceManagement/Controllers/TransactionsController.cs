using FinanceManagement.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.Security.Claims;
using FinanceManagement.Services;

namespace FinanceManagement.Controllers;

[Authorize]
public class TransactionsController : Controller
{
    private readonly ITransactionService _transactionService;

    public TransactionsController(
        ITransactionService transactionService)
    {
        _transactionService = transactionService;
    }

    [HttpGet]
    public async Task<IActionResult> Index(
    DateOnly? dateFrom,
    DateOnly? dateTo,
    int? accountId)
    {
        if (!int.TryParse(
                User.FindFirstValue(ClaimTypes.NameIdentifier),
                out var userId))
        {
            return Challenge();
        }

        var page = await _transactionService.GetHistoryAsync(
            userId,
            dateFrom,
            dateTo,
            accountId,
            ModelState.IsValid);

        if (page.AccountNotFound)
        {
            return NotFound();
        }

        if (page.ErrorMessage is not null)
        {
            ModelState.AddModelError(
                string.Empty, page.ErrorMessage);
        }

        HttpContext.Session.SetString(
            "LastTransactionsVisitUtc",
            DateTimeOffset.UtcNow.ToString("O"));

        ViewBag.DateFrom = dateFrom?.ToString("yyyy-MM-dd");
        ViewBag.DateTo = dateTo?.ToString("yyyy-MM-dd");
        ViewBag.CurrencyTotals = page.Totals;

        var accounts = page.Accounts
            .Select(a => new
            {
                a.Id,
                Label = a.Name + " (" + a.CurrencyCode + ")"
            })
            .ToList();

        ViewBag.Accounts = new SelectList(
            accounts, "Id", "Label", accountId);

        return View(page.Operations);
    }

    [HttpGet]
    public async Task<IActionResult> Create()
    {
        if (!int.TryParse(
            User.FindFirstValue(ClaimTypes.NameIdentifier),
            out var userId))
        {
            return Challenge();
        }

        await LoadListsAsync(userId);

        return View(new CreateTransactionViewModel());
    }

    [HttpPost]
    [ValidateAntiForgeryToken]

    public async Task<IActionResult> Create(
    CreateTransactionViewModel model)
    {
        if (!int.TryParse(
                User.FindFirstValue(ClaimTypes.NameIdentifier),
                out var userId))
        {
            return Challenge();
        }

        if (!ModelState.IsValid)
        {
            await LoadListsAsync(userId);
            return View(model);
        }

        var result = await _transactionService.CreateAsync(userId, model);

        if (!result.Success)
        {
            ModelState.AddModelError(
                result.Field ?? string.Empty,
                result.Error ?? "Не удалось сохранить операцию");

            await LoadListsAsync(userId);
            return View(model);
        }

        TempData["SuccessMessage"] = "Операция сохранена";

        return RedirectToAction("Index", "Home");
    }
    private async Task LoadListsAsync(int userId)
    {
        var accounts = await _transactionService
            .GetAccountsAsync(userId);

        var categories = await _transactionService
            .GetCategoriesAsync(userId);

        var accountItems = accounts.Select(a => new
        {
            a.Id,
            Label = a.Name + " (" + a.CurrencyCode + ")"
        }).ToList();

        var categoryItems = categories.Select(c => new
        {
            c.Id,
            Label = c.Name +
                (c.Type == TransactionType.Income
                    ? " — доход"
                    : " — расход")
        }).ToList();

        ViewBag.Accounts = new SelectList(
            accountItems, "Id", "Label");

        ViewBag.Categories = new SelectList(
            categoryItems, "Id", "Label");
    }

    [HttpGet]
    public async Task<IActionResult> Edit(int id)
    {
        if (!int.TryParse(
                User.FindFirstValue(ClaimTypes.NameIdentifier),
                out var userId))
        {
            return Challenge();
        }

        var model = await _transactionService.GetEditModelAsync(
            id, userId);

        if (model is null)
        {
            return NotFound();
        }

        await LoadListsAsync(userId);

        ViewBag.TransactionId = id;

        return View(model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(
        int id,
        CreateTransactionViewModel model)
    {
        if (!int.TryParse(
                User.FindFirstValue(ClaimTypes.NameIdentifier),
                out var userId))
        {
            return Challenge();
        }

        if (ModelState.IsValid)
        {
            var result = await _transactionService.EditAsync(
                id, userId, model);

            if (result.Success)
            {
                TempData["SuccessMessage"] = "Операция изменена";
                return RedirectToAction(nameof(Index));
            }

            ModelState.AddModelError(
                result.Field ?? string.Empty,
                result.Error ?? "Не удалось изменить операцию");
        }

        await LoadListsAsync(userId);
        ViewBag.TransactionId = id;

        return View(model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(int id)
    {
        if (!int.TryParse(
                User.FindFirstValue(ClaimTypes.NameIdentifier),
                out var userId))
        {
            return Challenge();
        }

        var result = await _transactionService.DeleteAsync(id, userId);

        if (!result.Success)
        {
            TempData["ErrorMessage"] = result.Error;
            return RedirectToAction(nameof(Index));
        }

        TempData["SuccessMessage"] = "Операция удалена";

        return RedirectToAction(nameof(Index));
    }
}
