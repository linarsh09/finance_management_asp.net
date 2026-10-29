using System.ComponentModel.DataAnnotations;

namespace FinanceManagement.Models;

public class User
{
    /// <summary>
    ///  Айди
    /// </summary>
    public int Id { get; set; }

    /// <summary>Уникальное имя для входа.</summary>
    [StringLength(50)]
    public string Login { get; set; } = string.Empty;

    /// <summary>
    ///  Почта
    /// </summary>
    public string Email { get; set; } = default!;

    /// <summary>
    ///  Пароль
    /// </summary>
    public string PasswordHash { get; set; } = default!;

    public ICollection<Account> Accounts { get; set; } = new List<Account>();
    public ICollection<Category> Categories { get; set; } = new List<Category>();
}
