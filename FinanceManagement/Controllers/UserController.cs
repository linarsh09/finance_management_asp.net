using FinanceManagement.Data;
using FinanceManagement.DTO;
using FinanceManagement.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
namespace FinanceManagement.Controllers;
public class UserController : Controller
{
    private readonly FinanceDbContext _db;
    private readonly PasswordHasher<User> _hasher = new();
    public UserController(FinanceDbContext db) => _db = db;
    [HttpGet] public IActionResult Registry() => View(new CreateUserDto());
    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Registry(CreateUserDto dto)
    {
        if (!ModelState.IsValid) return View(dto);
        var login = dto.Login.Trim().ToLowerInvariant();
        var email = dto.Email.Trim().ToLowerInvariant();
        if (await _db.Users.AnyAsync(u => u.Login == login)) { ModelState.AddModelError(nameof(dto.Login), "Этот логин уже занят."); return View(dto); }
        if (await _db.Users.AnyAsync(u => u.Email == email)) { ModelState.AddModelError(nameof(dto.Email), "Эта почта уже зарегистрирована."); return View(dto); }
        var user = new User { Login = login, Email = email };
        user.PasswordHash = _hasher.HashPassword(user, dto.Password);
        _db.Users.Add(user); await _db.SaveChangesAsync(); await SignIn(user);
        return RedirectToAction("Index", "Home");
    }
    [HttpGet] public IActionResult Login() => View(new LoginDto());
    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Login(LoginDto dto)
    {
        if (!ModelState.IsValid) return View(dto);
        var loginOrEmail = dto.LoginOrEmail.Trim().ToLowerInvariant();
        var user = await _db.Users.SingleOrDefaultAsync(u => u.Login == loginOrEmail || u.Email == loginOrEmail);
        if (user is null || _hasher.VerifyHashedPassword(user, user.PasswordHash, dto.Password) == PasswordVerificationResult.Failed) { ModelState.AddModelError(string.Empty, "Неверная почта или пароль."); return View(dto); }
        await SignIn(user); return RedirectToAction("Index", "Home");
    }
    [HttpPost, ValidateAntiForgeryToken] public async Task<IActionResult> Logout() { await HttpContext.SignOutAsync("Cookies"); return RedirectToAction("Index", "Home"); }
    private Task SignIn(User user) => HttpContext.SignInAsync("Cookies", new ClaimsPrincipal(new ClaimsIdentity(new[] { new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()), new Claim(ClaimTypes.Name, user.Login), new Claim(ClaimTypes.Email, user.Email) }, "Cookies")));
}
