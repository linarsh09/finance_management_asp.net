using System.ComponentModel.DataAnnotations;
using FinanceManagement.Validation;

namespace FinanceManagement.Models;

public class CreateCategoryViewModel
{
    [Required(ErrorMessage = "Введите название категории")]
    [StringLength(100,
        ErrorMessage = "Название должно содержать не более 100 символов")]
    [RegularExpression(ValidationPatterns.Name,
        ErrorMessage =
            "Начните название с буквы или цифры. " +
            "Допустимы буквы, цифры, пробелы и знаки . , ' ( ) -")]
    [Display(Name = "Название категории")]
    public string Name { get; set; } = string.Empty;

    [EnumDataType(typeof(TransactionType),
        ErrorMessage = "Выберите доход или расход")]
    [Display(Name = "Тип категории")]
    public TransactionType Type { get; set; } = TransactionType.Expense;
}