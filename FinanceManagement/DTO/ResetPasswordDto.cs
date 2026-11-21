using System.ComponentModel.DataAnnotations;

namespace FinanceManagement.DTO;

public class ResetPasswordDto
{
    [Required(ErrorMessage = "Введите почту")]
    [EmailAddress(ErrorMessage = "Введите корректную почту")]
    [StringLength(256)]
    [Display(Name = "Почта")]
    public string Email { get; set; } = string.Empty;

    [Required(ErrorMessage = "Введите код из письма")]
    [RegularExpression(@"^[0-9]{6}$",
        ErrorMessage = "Код должен содержать ровно 6 цифр")]
    [Display(Name = "Код")]
    public string Code { get; set; } = string.Empty;

    [Required(ErrorMessage = "Введите новый пароль")]
    [StringLength(100, MinimumLength = 8,
        ErrorMessage = "Пароль должен содержать от 8 до 100 символов")]
    [DataType(DataType.Password)]
    [Display(Name = "Новый пароль")]
    public string NewPassword { get; set; } = string.Empty;

    [Required(ErrorMessage = "Повторите пароль")]
    [Compare(nameof(NewPassword),
        ErrorMessage = "Пароли должны совпадать")]
    [DataType(DataType.Password)]
    [Display(Name = "Повторите новый пароль")]
    public string ConfirmPassword { get; set; } = string.Empty;
}