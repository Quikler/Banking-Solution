using Common;
using Common.DTOs;

namespace BAL.Services.Account;

public interface IAccountTransactionService
{
    Task<Result<bool, FailureDto>> DepositAsync(Guid userId, decimal amount);
    Task<Result<bool, FailureDto>> WithdrawAsync(Guid userId, decimal amount);
    Task<Result<bool, FailureDto>> TransferAsync(Guid fromUserId, Guid toUserId, decimal amount);
}
