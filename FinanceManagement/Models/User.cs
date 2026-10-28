namespace FinanceManagement.Models;

public class User
{
    /// <summary>
    ///  Айди
    /// </summary>
    public int Id { get; set; }

    /// <summary>
    ///  Почта
    /// </summary>
    public string Email { get; set; } = default!;

    /// <summary>
    ///  Пароль
    /// </summary>
    public string Password { get; set; } = default!;
}
