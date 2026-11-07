using FinanceManagement.Data;
using FinanceManagement.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using System.Security.Claims;

namespace FinanceManagement.Controllers;

[Authorize]
public class TransfersController : Controller
{
    private readonly FinanceDbContext _context;
    private readonly IMemoryCache _cache;

    public TransfersController(
        FinanceDbContext context,
        IMemoryCache cache)
    {
        _context = context;
        _cache = cache;
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

        await LoadAccountsAsync(userId);

        return View(new CreateTransferViewModel());
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(
        CreateTransferViewModel model)
    {
        if (!int.TryParse(
            User.FindFirstValue(ClaimTypes.NameIdentifier),
            out var userId))
        {
            return Challenge();
        }

        var fromAccount = await _context.Accounts
            .FirstOrDefaultAsync(a =>
                a.Id == model.FromAccountId && a.UserId == userId);

        var toAccount = await _context.Accounts
            .FirstOrDefaultAsync(a =>
                a.Id == model.ToAccountId && a.UserId == userId);

        if (fromAccount is null)
        {
            ModelState.AddModelError(
                nameof(model.FromAccountId),
                "Выберите свой счёт списания");
        }

        if (toAccount is null)
        {
            ModelState.AddModelError(
                nameof(model.ToAccountId),
                "Выберите свой счёт зачисления");
        }

        if (model.FromAccountId == model.ToAccountId)
        {
            ModelState.AddModelError(
                nameof(model.ToAccountId),
                "Выберите другой счёт для зачисления");
        }

        if (fromAccount is not null && toAccount is not null
            && !string.Equals(
                fromAccount.CurrencyCode,
                toAccount.CurrencyCode,
                StringComparison.OrdinalIgnoreCase))
        {
            ModelState.AddModelError(
                nameof(model.ToAccountId),
                "Для перевода выберите счета в одной валюте");
        }

        if (model.Date == default)
        {
            ModelState.AddModelError(
                nameof(model.Date), "Укажите дату");
        }

        if (decimal.Round(model.Amount, 2) != model.Amount)
        {
            ModelState.AddModelError(
                nameof(model.Amount),
                "Допускается не более двух знаков после запятой");
        }

        if (!ModelState.IsValid)
        {
            await LoadAccountsAsync(userId);
            return View(model);
        }

        var transfer = new Transfer
        {
            FromAccountId = model.FromAccountId,
            ToAccountId = model.ToAccountId,
            Amount = model.Amount,
            Date = model.Date,
            Note = model.Note?.Trim()
        };

        _context.Transfers.Add(transfer);
        await _context.SaveChangesAsync();

        _cache.Remove(HomeController.DashboardCacheKey(userId));

        TempData["SuccessMessage"] = "Перевод сохранён";

        return RedirectToAction("Index", "Home");
    }

    private async Task LoadAccountsAsync(int userId)
    {
        var accounts = await _context.Accounts
            .Where(a => a.UserId == userId)
            .OrderBy(a => a.Name)
            .Select(a => new
            {
                a.Id,
                Label = a.Name + " (" + a.CurrencyCode + ")"
            })
            .ToListAsync();

        ViewBag.FromAccounts = new SelectList(accounts, "Id", "Label");
        ViewBag.ToAccounts = new SelectList(accounts, "Id", "Label");
    }
}