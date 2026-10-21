using System.ComponentModel.DataAnnotations;
 
namespace FinanceManagement.Models
{
    public class CreateCategoryViewModel
    {
        [Required(ErrorMessage = "Введите название категории")]
        [StringLength(100)]
        [Display(Name = "Название категории")]
        public string Name { get; set; } = string.Empty;

        [EnumDataType(typeof(TransactionType),
            ErrorMessage = "Выберите категорию (доход/расход)")]

        [Display(Name = "Тип категории")]
        public TransactionType Type { get; set; } = TransactionType.Expense;
    }

}
