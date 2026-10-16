using FinanceManagement.Data;
using FinanceManagement.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;


namespace FinanceManagement.Controllers
{
    [Authorize]
    public class AccountsController : Controller
    {
        private readonly FinanceDbContext _context;
        private readonly UserManager<IdentityUser> _userManager;

        public AccountsController(
            FinanceDbContext context,
            UserManager<IdentityUser> userManager)
        {
            _context = context;
            _userManager = userManager;
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

            var userId = _userManager.GetUserId(User);

            if (userId is null)
            {
                return Challenge();
            }

            var account = new Account
            {
                Name = model.Name.Trim(),
                CurrencyCode = model.CurrencyCode,
                UserId = userId
            };

            _context.Accounts.Add(account);
            await _context.SaveChangesAsync();

            return RedirectToAction("Index", "Home");
        }
    }
}