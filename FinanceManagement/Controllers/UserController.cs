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

    public UserController(
        IUserService userService,
        IEmailConfirmationService emailConfirmation,
        IPasswordResetService passwordReset)
    {
        _userService = userService;
        _emailConfirmation = emailConfirmation;
        _passwordReset = passwordReset;
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

        var sending = await _emailConfirmation.SendCodeAsync(
            result.User.Email);

        if (sending.Success)
        {
            TempData["SuccessMessage"] =
                "Код отправлен на вашу почту.";
        }
        else
        {
            TempData["ErrorMessage"] = sending.Error;
        }

        return RedirectToAction(
            nameof(ConfirmEmail),
            new { email = result.User.Email });
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

        new Claim(ClaimTypes.Email, user.Email),

        new Claim(
            "SessionVersion",
            user.SessionVersion.ToString())
        };



        var identity = new ClaimsIdentity(
            claims,
            SessionAuthenticationHandler.SchemeName);

        return HttpContext.SignInAsync(
            SessionAuthenticationHandler.SchemeName,
            new ClaimsPrincipal(identity));
    }
    private readonly IEmailConfirmationService _emailConfirmation;

    [HttpGet]
    public IActionResult ConfirmEmail(string? email)
    {
        return View(new ConfirmEmailDto
        {
            Email = email ?? string.Empty
        });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ConfirmEmail(ConfirmEmailDto model)
    {
        if (!ModelState.IsValid)
        {
            return View(model);
        }

        var result = await _emailConfirmation.ConfirmAsync(model);

        if (!result.Success)
        {
            ModelState.AddModelError(
                result.Field ?? string.Empty,
                result.Error ?? "Не удалось подтвердить почту.");

            return View(model);
        }

        TempData["SuccessMessage"] =
            "Почта подтверждена. Теперь войдите в аккаунт.";

        return RedirectToAction(nameof(Login));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ResendEmailCode(
        RequestEmailCodeDto model)
    {
        if (!ModelState.IsValid)
        {
            return View(
                "ConfirmEmail",
                new ConfirmEmailDto { Email = model.Email });
        }

        var result = await _emailConfirmation.SendCodeAsync(model.Email);

        if (result.Success)
        {
            TempData["SuccessMessage"] = "Новый код отправлен.";
        }
        else
        {
            TempData["ErrorMessage"] = result.Error;
        }

        return RedirectToAction(
            nameof(ConfirmEmail),
            new { email = model.Email });
    }

    private readonly IPasswordResetService _passwordReset;

    [HttpGet]
    public IActionResult ForgotPassword()
    {
        return View(new ForgotPasswordDto());
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ForgotPassword(
        ForgotPasswordDto model)
    {
        if (!ModelState.IsValid)
        {
            return View(model);
        }

        var result = await _passwordReset.SendCodeAsync(model);

        if (!result.Success)
        {
            ModelState.AddModelError(
                result.Field ?? string.Empty,
                result.Error ?? "Проверьте введённые данные.");

            return View(model);
        }

        TempData["SuccessMessage"] =
            "Если адрес зарегистрирован и почта подтверждена, " +
            "на него отправлен код. Проверьте также папку «Спам». " +
            "Повторный запрос доступен через минуту.";

        return RedirectToAction(
            nameof(ResetPassword),
            new { email = model.Email.Trim() });
    }

    [HttpGet]
    public IActionResult ResetPassword(string? email)
    {
        return View(new ResetPasswordDto
        {
            Email = email ?? string.Empty
        });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ResetPassword(
        ResetPasswordDto model)
    {
        if (!ModelState.IsValid)
        {
            return View(model);
        }

        var result = await _passwordReset.ResetAsync(model);

        if (!result.Success)
        {
            ModelState.AddModelError(
                result.Field ?? string.Empty,
                result.Error ?? "Не удалось изменить пароль.");

            return View(model);
        }

        await HttpContext.SignOutAsync(
            SessionAuthenticationHandler.SchemeName);

        TempData["SuccessMessage"] =
            "Пароль изменён. Войдите с новым паролем.";

        return RedirectToAction(nameof(Login));
    }
}