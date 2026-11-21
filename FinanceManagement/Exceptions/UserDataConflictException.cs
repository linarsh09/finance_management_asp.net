namespace FinanceManagement.Exceptions;

public class UserDataConflictException : Exception
{
    public UserDataConflictException(Exception innerException)
        : base("Данные пользователя изменились одновременно.", innerException)
    {
    }
}