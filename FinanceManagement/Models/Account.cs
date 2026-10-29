using System.ComponentModel.DataAnnotations;
namespace FinanceManagement.Models;
public class Account
{
    public int Id { get; set; }
    [Required, StringLength(100)] public string Name { get; set; } = string.Empty;
    [Required, StringLength(3)] public string CurrencyCode { get; set; } = "KZT";
    public int UserId { get; set; }
    public User User { get; set; } = null!;
    public ICollection<Transaction> Transactions { get; set; } = new List<Transaction>();
}
