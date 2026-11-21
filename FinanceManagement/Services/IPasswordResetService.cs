using FinanceManagement.DTO;

namespace FinanceManagement.Services;

public interface IPasswordResetService
{
    Task<OperationResult> SendCodeAsync(ForgotPasswordDto model);

    Task<OperationResult> ResetAsync(ResetPasswordDto model);
}