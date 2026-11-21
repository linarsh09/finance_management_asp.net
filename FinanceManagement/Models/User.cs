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


    public bool EmailConfirmed { get; set; }

    [StringLength(500)]
    public string? EmailConfirmationCodeHash { get; set; }

    public DateTimeOffset? EmailConfirmationExpiresAt { get; set; }

    public int EmailConfirmationAttempts { get; set; }

    public DateTimeOffset? EmailConfirmationLastSentAt { get; set; }



    [StringLength(500)]
    public string? PasswordResetCodeHash { get; set; }

    public DateTimeOffset? PasswordResetExpiresAt { get; set; }

    public int PasswordResetAttempts { get; set; }

    public DateTimeOffset? PasswordResetLastSentAt { get; set; }

    public int SessionVersion { get; set; }
}
