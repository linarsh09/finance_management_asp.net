using System.ComponentModel.DataAnnotations;

namespace FinanceManagement.DTO;

public class RequestEmailCodeDto
{
    [Required(ErrorMessage = "Укажите почту")]
    [EmailAddress(ErrorMessage = "Введите корректную почту")]
    [StringLength(256)]
    public string Email { get; set; } = string.Empty;
}