using FinanceManagement.DTO;
using FinanceManagement.Services;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using FinanceManagement.Authentication;

namespace FinanceManagement.Controllers;

public class UserController : Controller
{
    private readonly IUserService _userService;

    public UserController(IUserService userService)
    {
        _userService = userService;
    }

    [HttpGet]
    public IActionResult Registry()
    {
        return View(new CreateUserDto());
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Registry(CreateUserDto dto)
    {
        if (!ModelState.IsValid)
        {
            return View(dto);
        }

        var result = await _userService.RegisterAsync(dto);

        if (!result.Success || result.User is null)
        {
            ModelState.AddModelError(
                result.Field ?? string.Empty,
                result.Error ?? "Не удалось зарегистрироваться.");

            return View(dto);
        }

        await SignInAsync(result.User);

        return RedirectToAction("Index", "Home");
    }

    [HttpGet]
    public IActionResult Login()
    {
        return View(new LoginDto());
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Login(LoginDto dto)
    {
        if (!ModelState.IsValid)
        {
            return View(dto);
        }

        var result = await _userService.LoginAsync(dto);

        if (!result.Success || result.User is null)
        {
            ModelState.AddModelError(
                result.Field ?? string.Empty,
                result.Error ?? "Не удалось войти.");

            return View(dto);
        }

        await SignInAsync(result.User);

        return RedirectToAction("Index", "Home");
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Logout()
    {
        await HttpContext.SignOutAsync(
            SessionAuthenticationHandler.SchemeName);

        return RedirectToAction(nameof(Login));
    }

    private Task SignInAsync(AuthenticatedUser user)
    {
        var claims = new[]
        {
        new Claim(
            ClaimTypes.NameIdentifier,
            user.Id.ToString()),

        new Claim(ClaimTypes.Name, user.Login),

        new Claim(ClaimTypes.Email, user.Email)
    };

        var identity = new ClaimsIdentity(
            claims,
            SessionAuthenticationHandler.SchemeName);

        return HttpContext.SignInAsync(
            SessionAuthenticationHandler.SchemeName,
            new ClaimsPrincipal(identity));
    }
}