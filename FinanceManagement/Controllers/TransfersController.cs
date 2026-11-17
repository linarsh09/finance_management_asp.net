using FinanceManagement.Models;
using FinanceManagement.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.Security.Claims;

namespace FinanceManagement.Controllers;

[Authorize]
public class TransfersController : Controller
{
    private readonly ITransferService _transferService;

    public TransfersController(ITransferService transferService)
    {
        _transferService = transferService;
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

        if (ModelState.IsValid)
        {
            var result = await _transferService.CreateAsync(
                userId, model);

            if (result.Success)
            {
                TempData["SuccessMessage"] = "Перевод сохранён";

                return RedirectToAction("Index", "Home");
            }

            ModelState.AddModelError(
                result.Field ?? string.Empty,
                result.Error ?? "Не удалось сохранить перевод");
        }

        await LoadAccountsAsync(userId);

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

        var result = await _transferService.DeleteAsync(id, userId);

        if (result.Success)
        {
            TempData["SuccessMessage"] = "Перевод удалён";
        }
        else
        {
            TempData["ErrorMessage"] =
                result.Error ?? "Не удалось удалить перевод";
        }

        return RedirectToAction("Index", "Transactions");
    }

    private async Task LoadAccountsAsync(int userId)
    {
        var accounts = await _transferService.GetAccountsAsync(userId);

        var items = accounts
            .Select(a => new
            {
                a.Id,
                Label = a.Name + " (" + a.CurrencyCode + ")"
            })
            .ToList();

        ViewBag.FromAccounts = new SelectList(items, "Id", "Label");
        ViewBag.ToAccounts = new SelectList(items, "Id", "Label");
    }
}