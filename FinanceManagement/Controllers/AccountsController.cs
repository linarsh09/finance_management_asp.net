using FinanceManagement.Data;
using FinanceManagement.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;


namespace FinanceManagement.Controllers
{
    [Authorize]
    public class AccountsController : Controller
    {
        private readonly FinanceDbContext _context;
        

        public AccountsController(FinanceDbContext context)
        {
            _context = context;

        }

        [HttpGet]
        public IActionResult Create()
        {
            return View(new CreateAccountViewModel());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(CreateAccountViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var userId = int.TryParse(User.FindFirstValue(ClaimTypes.NameIdentifier), out var parsedUserId) ? parsedUserId : (int?)null;

            if (userId is null)
            {
                return Challenge();
            }

            var account = new Account
            {
                Name = model.Name.Trim(),
                CurrencyCode = model.CurrencyCode,
                UserId = userId.Value
            };

            _context.Accounts.Add(account);
            await _context.SaveChangesAsync();

            return RedirectToAction("Index", "Home");
        }
    }
}