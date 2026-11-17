namespace FinanceManagement.Validation;

public static class ValidationPatterns
{
    public const string Login = @"^[A-Za-z0-9_.-]+$";

    public const string Email = @"^[^@\s]+@[^@\s]+\.[^@\s]+$";

    public const string Name =
        @"^[A-Za-zА-Яа-яЁёӘәҒғҚқҢңӨөҰұҮүҺһІі0-9]" +
        @"[A-Za-zА-Яа-яЁёӘәҒғҚқҢңӨөҰұҮүҺһІі0-9 .,'()\-]*$";
}