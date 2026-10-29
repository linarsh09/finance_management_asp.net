using System.ComponentModel.DataAnnotations;

namespace FinanceManagement.Models;

public class CreateTransactionViewModel
{
    [Range(1, int.MaxValue, ErrorMessage = "Выберите счёт")]
    [Display(Name = "Счёт")]
    public int AccountId { get; set; }

    [Range(1, int.MaxValue, ErrorMessage = "Выберите категорию")]
    [Display(Name = "Категория")]
    public int CategoryId { get; set; }

    [Range(typeof(decimal), "0.01", "999999999.99",
        ParseLimitsInInvariantCulture = true,
        ErrorMessage = "Введите сумму от 0,01 до 999 999 999,99")]
    [Display(Name = "Сумма")]
    public decimal Amount { get; set; }

    [DataType(DataType.Date)]
    [Display(Name = "Дата")]
    public DateOnly Date { get; set; } =
        DateOnly.FromDateTime(DateTime.Today);

    [StringLength(500)]
    [Display(Name = "Примечание")]
    public string? Note { get; set; }
}