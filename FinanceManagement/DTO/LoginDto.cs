using System.ComponentModel.DataAnnotations;
namespace FinanceManagement.DTO;
public class LoginDto
{
    [Required, StringLength(256)]
    [Display(Name = "Логин или Email")]
    public string LoginOrEmail { get; set; } = string.Empty;
    [Required] public string Password { get; set; } = string.Empty;
}
