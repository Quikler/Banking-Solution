using Common;
using Common.DTOs;
using DAL;
using DAL.Entities;
using Microsoft.EntityFrameworkCore;

namespace BAL.Services.Account;

public class AccountTransactionService(AppDbContext context) : IAccountTransactionService
{
    public async Task<Result<bool, FailureDto>> DepositAsync(Guid userId, decimal amount)
    {
        var balance = await GetBalanceAsync(userId);
        if (balance is null)
        {
            return FailureDto.NotFound("User's balance not found");
        }

        if (amount <= 0)
        {
            return FailureDto.BadRequest("Amount must be positive");
        }
        balance.Balance += amount;

        int count = await context.SaveChangesAsync();
        return count > 0 ? true : FailureDto.BadRequest("Something went wrong during transfer");
    }

    public async Task<Result<bool, FailureDto>> WithdrawAsync(Guid userId, decimal amount)
    {
        var balance = await GetBalanceAsync(userId);
        if (balance is null)
        {
            return FailureDto.NotFound("User's balance not found");
        }

        if (amount <= 0)
        {
            return FailureDto.BadRequest("Amount must be positive");
        }

        if (balance.Balance < amount)
        {
            return FailureDto.BadRequest("Insufficient funds");
        }
        balance.Balance -= amount;

        int count = await context.SaveChangesAsync();
        return count > 0 ? true : FailureDto.BadRequest("Something went wrong during transfer");
    }

    public async Task<Result<bool, FailureDto>> TransferAsync(Guid fromUserId, Guid toUserId, decimal amount)
    {
        if (fromUserId == toUserId)
        {
            return FailureDto.BadRequest("Cannot transfer to the same user");
        }

        var fromBalance = await GetBalanceAsync(fromUserId);
        if (fromBalance is null)
        {
            return FailureDto.NotFound("User's balance which transfers not found");
        }

        var toBalance = await GetBalanceAsync(toUserId);
        if (toBalance is null)
        {
            return FailureDto.NotFound("User's balance which accepts not found");
        }

        if (fromBalance.Balance < amount)
        {
            return FailureDto.BadRequest("Insufficient funds for transfer");
        }

        fromBalance.Balance -= amount;
        toBalance.Balance += amount;

        int count = await context.SaveChangesAsync();
        return count > 0 ? true : FailureDto.BadRequest("Something went wrong during transfer");
    }

    private async Task<BalanceEntity?> GetBalanceAsync(Guid userId)
    {
        var balance = await context.Balances
            .FirstOrDefaultAsync(b => b.UserId == userId);

        return balance;
    }
}
