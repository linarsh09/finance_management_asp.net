using System.ComponentModel.DataAnnotations;

namespace FinanceManagement.DTO;

public class ForgotPasswordDto
{
    [Required(ErrorMessage = "Введите почту")]
    [EmailAddress(ErrorMessage = "Введите корректную почту")]
    [StringLength(256)]
    [Display(Name = "Почта")]
    public string Email { get; set; } = string.Empty;
}