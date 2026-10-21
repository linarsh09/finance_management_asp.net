using FinanceManagement.Data;
using FinanceManagement.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace FinanceManagement.Controllers;

[Authorize]
public class CategoriesController : Controller
{
    private readonly FinanceDbContext _context;
    private readonly UserManager<IdentityUser> _userManager;

    public CategoriesController(FinanceDbContext context, UserManager<IdentityUser> userManager)
    {
        _context = context;
        _userManager = userManager;
    }

    [HttpGet]
    public IActionResult Create() 
    {
        return View(new CreateCategoryViewModel());
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(CreateCategoryViewModel model)
    {
        if (!ModelState.IsValid)
        {
            return View(model);
        }

        var userId = _userManager.GetUserId(User);

        if (userId is null)
        {
            return Challenge();
        }

        var category = new Category
        {
            Name = model.Name.Trim(),
            Type = model.Type,
            UserId = userId
        };

        _context.Categories.Add(category);
        await _context.SaveChangesAsync();

        return RedirectToAction("Index", "Home");
    }

}
