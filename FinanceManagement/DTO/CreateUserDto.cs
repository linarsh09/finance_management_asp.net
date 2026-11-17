using System.ComponentModel.DataAnnotations;
using FinanceManagement.Validation;

namespace FinanceManagement.DTO;

public class CreateUserDto
{
    [Required(ErrorMessage = "Введите логин")]
    [StringLength(50, MinimumLength = 3,
        ErrorMessage = "Логин должен содержать от 3 до 50 символов")]
    [RegularExpression(ValidationPatterns.Login,
        ErrorMessage =
            "Допустимы латинские буквы, цифры, точка, дефис и подчёркивание")]
    [Display(Name = "Логин")]
    public string Login { get; set; } = string.Empty;

    [Required(ErrorMessage = "Введите почту")]
    [StringLength(256)]
    [EmailAddress(ErrorMessage = "Введите корректную почту")]
    [RegularExpression(ValidationPatterns.Email,
        ErrorMessage = "Введите почту в формате name@example.com")]
    [Display(Name = "Почта")]
    public string Email { get; set; } = string.Empty;

    [Required(ErrorMessage = "Введите пароль")]
    [StringLength(100, MinimumLength = 8,
        ErrorMessage = "Пароль должен содержать от 8 до 100 символов")]
    [DataType(DataType.Password)]
    [Display(Name = "Пароль")]
    public string Password { get; set; } = string.Empty;

    [Required(ErrorMessage = "Повторите пароль")]
    [Compare(nameof(Password),
        ErrorMessage = "Пароли должны совпадать")]
    [DataType(DataType.Password)]
    [Display(Name = "Повторите пароль")]
    public string ConfirmPassword { get; set; } = string.Empty;
}