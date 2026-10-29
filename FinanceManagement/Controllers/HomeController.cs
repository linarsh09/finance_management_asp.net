using FinanceManagement.Data;
using FinanceManagement.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Diagnostics;
using System.Security.Claims;
namespace FinanceManagement.Controllers;
public class HomeController : Controller
{
    private readonly FinanceDbContext _context;
    public HomeController(FinanceDbContext context) => _context = context;
    [Authorize]
    public async Task<IActionResult> Index()
    {
        if (!int.TryParse(User.FindFirstValue(ClaimTypes.NameIdentifier), out var userId)) return Challenge();
        var accounts = await _context.Accounts.Where(a => a.UserId == userId).OrderBy(a => a.Name).ToListAsync();
        return View(accounts);
    }
    public IActionResult Privacy() => View();
    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error() => View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
}
