namespace FinanceManagement.Repositories;

public interface IDatabaseInitializerRepository
{
    Task InitializeAsync();
}