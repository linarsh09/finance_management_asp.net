using FinanceManagement.Data;
using FinanceManagement.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace FinanceManagement.Controllers;

[Authorize]
public class TransactionsController : Controller
{
    private readonly FinanceDbContext _context;
    private readonly IMemoryCache _cache;

    public TransactionsController(FinanceDbContext context, IMemoryCache cache)
    {
        _context = context;
        _cache = cache;
    }
    [HttpGet]
    public async Task<IActionResult> Index(
    DateOnly? dateFrom,
    DateOnly? dateTo)
    {
        if (!int.TryParse(
            User.FindFirstValue(ClaimTypes.NameIdentifier),
            out var userId))
        {
            return Challenge();
        }

        var transactionsQuery = _context.Transactions
            .AsNoTracking()
            .Where(t => t.Account.UserId == userId);

        var transfersQuery = _context.Transfers
            .AsNoTracking()
            .Where(t => t.FromAccount.UserId == userId
                     && t.ToAccount.UserId == userId);

        if (dateFrom.HasValue && dateTo.HasValue
            && dateFrom.Value > dateTo.Value)
        {
            ModelState.AddModelError(
                string.Empty,
                "Начальная дата не должна быть позже конечной");
        }

        if (ModelState.IsValid)
        {
            if (dateFrom.HasValue)
            {
                transactionsQuery = transactionsQuery
                    .Where(t => t.Date >= dateFrom.Value);

                transfersQuery = transfersQuery
                    .Where(t => t.Date >= dateFrom.Value);
            }

            if (dateTo.HasValue)
            {
                transactionsQuery = transactionsQuery
                    .Where(t => t.Date <= dateTo.Value);

                transfersQuery = transfersQuery
                    .Where(t => t.Date <= dateTo.Value);
            }
        }

        var transactions = await transactionsQuery
            .Select(t => new OperationHistoryViewModel
            {
                Id = t.Id,
                IsTransfer = false,
                Date = t.Date,
                TypeLabel = t.Type == TransactionType.Income
                    ? "Доход"
                    : "Расход",
                AccountLabel = t.Account.Name,
                CategoryName = t.Category.Name,
                Amount = t.Amount,
                CurrencyCode = t.Account.CurrencyCode,
                Note = t.Note
            })
            .ToListAsync();

        var transfers = await transfersQuery
            .Select(t => new OperationHistoryViewModel
            {
                Id = t.Id,
                IsTransfer = true,
                Date = t.Date,
                TypeLabel = "Перевод",
                AccountLabel = t.FromAccount.Name
                    + " → " + t.ToAccount.Name,
                CategoryName = null,
                Amount = t.Amount,
                CurrencyCode = t.FromAccount.CurrencyCode,
                Note = t.Note
            })
            .ToListAsync();

        var history = transactions
            .Concat(transfers)
            .OrderByDescending(t => t.Date)
            .ThenByDescending(t => t.Id)
            .ThenBy(t => t.IsTransfer)
            .ToList();

        HttpContext.Session.SetString(
            "LastTransactionsVisitUtc",
            DateTimeOffset.UtcNow.ToString("O"));

        ViewBag.DateFrom = dateFrom?.ToString("yyyy-MM-dd");
        ViewBag.DateTo = dateTo?.ToString("yyyy-MM-dd");

        return View(history);
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

        var accountExists = await _context.Accounts
            .AnyAsync(a => a.Id == model.AccountId
                        && a.UserId == userId);

        var category = await _context.Categories
            .FirstOrDefaultAsync(c => c.Id == model.CategoryId
                                  && c.UserId == userId);

        if (!accountExists)
        {
            ModelState.AddModelError(
                nameof(model.AccountId),
                "Выберите свой счёт");
        }

        if (category is null)
        {
            ModelState.AddModelError(
                nameof(model.CategoryId),
                "Выберите свою категорию");
        }

        if (model.Date == default)
        {
            ModelState.AddModelError(
                nameof(model.Date),
                "Укажите дату");
        }

        if (decimal.Round(model.Amount, 2) != model.Amount)
        {
            ModelState.AddModelError(
                nameof(model.Amount),
                "Допускается не более двух знаков после запятой");
        }

        if (!ModelState.IsValid || category is null)
        {
            await LoadListsAsync(userId);
            return View(model);
        }

        var transaction = new Transaction
        {
            AccountId = model.AccountId,
            CategoryId = category.Id,
            Amount = model.Amount,
            Date = model.Date,
            Type = category.Type,
            Note = model.Note?.Trim()
        };

        _context.Transactions.Add(transaction);
        await _context.SaveChangesAsync();
        _cache.Remove(HomeController.DashboardCacheKey(userId));

        TempData["SuccessMessage"] = "Операция сохранена";

        return RedirectToAction("Index", "Home");
    }

    private async Task LoadListsAsync(int userId)
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

        var categories = await _context.Categories
            .Where(c => c.UserId == userId)
            .OrderBy(c => c.Name)
            .Select(c => new
            {
                c.Id,
                Label = c.Name +
                    (c.Type == TransactionType.Income
                        ? " — доход"
                        : " — расход")
            })
            .ToListAsync();

        ViewBag.Accounts = new SelectList(accounts, "Id", "Label");
        ViewBag.Categories = new SelectList(categories, "Id", "Label");
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

        var transaction = await _context.Transactions
            .AsNoTracking()
            .FirstOrDefaultAsync(t =>
                t.Id == id && t.Account.UserId == userId);

        if (transaction is null)
        {
            return NotFound();
        }

        var model = new CreateTransactionViewModel
        {
            AccountId = transaction.AccountId,
            CategoryId = transaction.CategoryId,
            Amount = transaction.Amount,
            Date = transaction.Date,
            Note = transaction.Note
        };

        await LoadListsAsync(userId);

        ViewBag.TransactionId = transaction.Id;

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

        var transaction = await _context.Transactions
            .FirstOrDefaultAsync(t =>
                t.Id == id && t.Account.UserId == userId);

        if (transaction is null)
        {
            return NotFound();
        }

        var accountExists = await _context.Accounts
            .AnyAsync(a => a.Id == model.AccountId
                        && a.UserId == userId);

        var category = await _context.Categories
            .FirstOrDefaultAsync(c => c.Id == model.CategoryId
                                  && c.UserId == userId);

        if (!accountExists)
        {
            ModelState.AddModelError(
                nameof(model.AccountId), "Выберите свой счёт");
        }

        if (category is null)
        {
            ModelState.AddModelError(
                nameof(model.CategoryId), "Выберите свою категорию");
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

        if (!ModelState.IsValid || category is null)
        {
            await LoadListsAsync(userId);
            ViewBag.TransactionId = id;
            return View(model);
        }

        transaction.AccountId = model.AccountId;
        transaction.CategoryId = category.Id;
        transaction.Amount = model.Amount;
        transaction.Date = model.Date;
        transaction.Type = category.Type;
        transaction.Note = model.Note?.Trim();

        await _context.SaveChangesAsync();
        _cache.Remove(HomeController.DashboardCacheKey(userId));

        TempData["SuccessMessage"] = "Операция изменена";

        return RedirectToAction(nameof(Index));
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

        var transaction = await _context.Transactions
            .FirstOrDefaultAsync(t =>
                t.Id == id && t.Account.UserId == userId);

        if (transaction is null)
        {
            return NotFound();
        }

        _context.Transactions.Remove(transaction);
        await _context.SaveChangesAsync();
        _cache.Remove(HomeController.DashboardCacheKey(userId));

        TempData["SuccessMessage"] = "Операция удалена";

        return RedirectToAction(nameof(Index));
    }
}
