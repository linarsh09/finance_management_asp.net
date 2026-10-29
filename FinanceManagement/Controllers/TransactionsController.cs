using FinanceManagement.Data;
using FinanceManagement.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace FinanceManagement.Controllers;

[Authorize]
public class TransactionsController : Controller
{
    private readonly FinanceDbContext _context;

    public TransactionsController(FinanceDbContext context)
    {
        _context = context;
    }
    [HttpGet]
    public async Task<IActionResult> Index()
    {
        if (!int.TryParse(
            User.FindFirstValue(ClaimTypes.NameIdentifier),
            out var userId))
        {
            return Challenge();
        }

        var transactions = await _context.Transactions
            .Where(t => t.Account.UserId == userId)
            .Include(t => t.Account)
            .Include(t => t.Category)
            .OrderByDescending(t => t.Date)
            .ThenByDescending(t => t.Id)
            .ToListAsync();

        return View(transactions);
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
}