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
            return FailureDto.NotFound("Balance for user not found");
        }

        if (amount <= 0)
        {
            return FailureDto.BadRequest("Amount must be greater than zero");
        }
        balance.Balance += amount;

        int count = await context.SaveChangesAsync();
        return count > 0 ? true : FailureDto.BadRequest("Failed to complete the deposit");
    }

    public async Task<Result<bool, FailureDto>> WithdrawAsync(Guid userId, decimal amount)
    {
        var balance = await GetBalanceAsync(userId);
        if (balance is null)
        {
            return FailureDto.NotFound("Balance for user not found");
        }

        if (amount <= 0)
        {
            return FailureDto.BadRequest("Amount must be greater than zero");
        }

        if (balance.Balance < amount)
        {
            return FailureDto.BadRequest("Not enough funds");
        }
        balance.Balance -= amount;

        int count = await context.SaveChangesAsync();
        return count > 0 ? true : FailureDto.BadRequest("Failed to complete the withdrawal");
    }

    public async Task<Result<bool, FailureDto>> TransferAsync(Guid fromUserId, Guid toUserId, decimal amount)
    {
        if (fromUserId == toUserId)
        {
            return FailureDto.BadRequest("Cannot transfer funds to the same user");
        }

        var fromBalance = await GetBalanceAsync(fromUserId);
        if (fromBalance is null)
        {
            return FailureDto.NotFound("Balance for sender user not found");
        }

        var toBalance = await GetBalanceAsync(toUserId);
        if (toBalance is null)
        {
            return FailureDto.NotFound("Balance for recipient user not found");
        }

        if (fromBalance.Balance < amount)
        {
            return FailureDto.BadRequest("Not enough funds for transfer");
        }

        fromBalance.Balance -= amount;
        toBalance.Balance += amount;

        int count = await context.SaveChangesAsync();
        return count > 0 ? true : FailureDto.BadRequest("Failed to complete the transfer");
    }

    private async Task<BalanceEntity?> GetBalanceAsync(Guid userId)
    {
        var balance = await context.Balances
            .FirstOrDefaultAsync(b => b.UserId == userId);

        return balance;
    }
}
