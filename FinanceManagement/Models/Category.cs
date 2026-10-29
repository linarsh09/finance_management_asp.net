using System.ComponentModel.DataAnnotations;
namespace FinanceManagement.Models;
public class Category
{
    public int Id { get; set; }
    [Required, StringLength(100)] public string Name { get; set; } = string.Empty;
    public TransactionType Type { get; set; }
    public int UserId { get; set; }
    public User User { get; set; } = null!;
    public ICollection<Transaction> Transactions { get; set; } = new List<Transaction>();
}
