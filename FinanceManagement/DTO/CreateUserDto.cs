using System.ComponentModel.DataAnnotations;
namespace FinanceManagement.DTO;
public class CreateUserDto
{
    [Required, StringLength(50, MinimumLength = 3)]
    [RegularExpression(@"^[a-zA-Z0-9_.-]+$", ErrorMessage = "Логин может содержать латинские буквы, цифры, точку, дефис и подчёркивание.")]
    public string Login { get; set; } = string.Empty;

    [Required, EmailAddress, StringLength(256)] public string Email { get; set; } = string.Empty;
    [Required, MinLength(8), StringLength(100)] public string Password { get; set; } = string.Empty;

    [Required, Compare(nameof(Password), ErrorMessage = "Пароли должны совпадать.")]
    [Display(Name = "Повторите пароль")]
    public string ConfirmPassword { get; set; } = string.Empty;
}
