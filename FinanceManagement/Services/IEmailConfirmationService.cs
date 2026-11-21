using FinanceManagement.DTO;

namespace FinanceManagement.Services;

public interface IEmailConfirmationService
{
    Task<OperationResult> SendCodeAsync(string email);

    Task<OperationResult> ConfirmAsync(ConfirmEmailDto model);
}