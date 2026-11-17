using System.ComponentModel.DataAnnotations;
using FinanceManagement.Validation;

namespace FinanceManagement.Models;

public class CreateAccountViewModel
{
    [Required(ErrorMessage = "Введите название счёта")]
    [StringLength(100,
        ErrorMessage = "Название должно содержать не более 100 символов")]
    [RegularExpression(ValidationPatterns.Name,
        ErrorMessage =
            "Начните название с буквы или цифры. " +
            "Допустимы буквы, цифры, пробелы и знаки . , ' ( ) -")]
    [Display(Name = "Название счёта")]
    public string Name { get; set; } = string.Empty;

    [Required(ErrorMessage = "Укажите валюту")]
    [RegularExpression("^(KZT|USD|EUR)$",
        ErrorMessage = "Выберите KZT, USD или EUR")]
    [Display(Name = "Валюта")]
    public string CurrencyCode { get; set; } = "KZT";
}