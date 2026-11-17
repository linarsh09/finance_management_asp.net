using FinanceManagement.Models;
using FinanceManagement.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace FinanceManagement.Controllers;

[Authorize]
public class CategoriesController : Controller
{
    private readonly ICategoryService _categoryService;

    public CategoriesController(ICategoryService categoryService)
    {
        _categoryService = categoryService;
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

        var categories = await _categoryService
            .GetByUserIdAsync(userId);

        return View(categories);
    }

    [HttpGet]
    public IActionResult Create()
    {
        return View(new CreateCategoryViewModel());
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(
        CreateCategoryViewModel model)
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

        await _categoryService.CreateAsync(userId, model);

        return RedirectToAction(nameof(Index));
    }
}