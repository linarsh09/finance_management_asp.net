using FinanceManagement.Models;
using FinanceManagement.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace FinanceManagement.Controllers;

[Authorize]
public class AccountsController : Controller
{
    private readonly IAccountService _accountService;

    public AccountsController(IAccountService accountService)
    {
        _accountService = accountService;
    }

    [HttpGet]
    public IActionResult Create()
    {
        return View(new CreateAccountViewModel());
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(
        CreateAccountViewModel model)
    {
        if (!int.TryParse(
            User.FindFirstValue(ClaimTypes.NameIdentifier),
            out var userId))
        {
            return Challenge();
        }

        if (!ModelState.IsValid)
        {
            return View(model);
        }

        await _accountService.CreateAsync(userId, model);

        return RedirectToAction("Index", "Home");
    }
}