using System.ComponentModel.DataAnnotations;

namespace FinanceManagement.DTO;

public class ConfirmEmailDto
{
    [Required(ErrorMessage = "Укажите почту")]
    [EmailAddress(ErrorMessage = "Введите корректную почту")]
    [StringLength(256)]
    [Display(Name = "Почта")]
    public string Email { get; set; } = string.Empty;

    [Required(ErrorMessage = "Введите код из письма")]
    [RegularExpression(@"^[0-9]{6}$",
        ErrorMessage = "Код должен содержать ровно 6 цифр")]
    [Display(Name = "Код подтверждения")]
    public string Code { get; set; } = string.Empty;
}