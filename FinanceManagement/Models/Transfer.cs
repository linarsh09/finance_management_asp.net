using System.ComponentModel.DataAnnotations;

namespace FinanceManagement.Models;

public class Transfer
{
    public int Id { get; set; }

    public int FromAccountId { get; set; }
    public Account FromAccount { get; set; } = null!;

    public int ToAccountId { get; set; }
    public Account ToAccount { get; set; } = null!;

    [Range(typeof(decimal), "0.01", "999999999.99",
        ParseLimitsInInvariantCulture = true,
        ErrorMessage = "Введите сумму от 0,01 до 999 999 999,99")]
    public decimal Amount { get; set; }

    public DateOnly Date { get; set; }

    [StringLength(500)]
    public string? Note { get; set; }
}