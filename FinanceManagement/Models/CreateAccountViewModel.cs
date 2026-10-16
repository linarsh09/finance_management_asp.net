using System.ComponentModel.DataAnnotations;

namespace FinanceManagement.Models;

public class CreateAccountViewModel
{
    [Required(ErrorMessage = "Введите название счёта")]
    [StringLength(100)]
    [Display(Name = "Название счёта")]
    public string Name { get; set; } = string.Empty;

    [Required(ErrorMessage = "Укажите валюту")]
    [RegularExpression(
        "^(KZT|USD|EUR)$",
        ErrorMessage = "Выберите KZT, USD или EUR")]
    [Display(Name = "Валюта")]
    public string CurrencyCode { get; set; } = "KZT";
}