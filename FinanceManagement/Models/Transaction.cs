using System.ComponentModel.DataAnnotations;

namespace FinanceManagement.Models;

public enum TransactionType
{
    Income = 1,
    Expense = 2
}

public class Transaction
{
    public int Id { get; set; }

    [Range(0.01, 9999999999999999.99)]
    public decimal Amount { get; set; }

    public DateOnly Date { get; set; }

    public TransactionType Type { get; set; }

    public int AccountId { get; set; }

    public Account Account { get; set; } = null!;

    public int CategoryId { get; set; }

    public Category Category { get; set; } = null!;

    [StringLength(500)]
    public string? Note { get; set; }
}