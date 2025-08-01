using BAL.Services.Account;
using Contracts;
using Contracts.V1.Requests.Transactions;
using Contracts.V1.Responses;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WebApi.Extensions;

namespace WebApi.Controllers.V1;

/// <summary>
/// Provides API endpoints for deposit, withdrawal, and transfer operations.
/// </summary>
[ApiController]
[Authorize]
public class AccountTransactionsController(IAccountTransactionService accountTransactionService) : AuthorizeController
{
    /// <summary>
    /// Deposits funds into a user's account.
    /// </summary>
    /// <response code="200">Deposit successful.</response>
    /// <response code="400">Bad request (e.g. invalid amount).</response>
    /// <response code="404">User balance not found.</response>
    [HttpPost(ApiRoutes.AccountTransaction.Deposit)]
    [ProducesResponseType(typeof(SuccessResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(FailureResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(FailureResponse), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Deposit([FromBody] DepositRequest request)
    {
        var result = await accountTransactionService.DepositAsync(UserId, request.Amount);
        return result.Match(
            _ => Ok(new SuccessResponse("Deposit successful")),
            failure => failure.ToActionResult()
        );
    }

    /// <summary>
    /// Withdraws funds from a user's account.
    /// </summary>
    /// <response code="200">Withdrawal successful.</response>
    /// <response code="400">Bad request (e.g. insufficient funds).</response>
    /// <response code="404">User balance not found.</response>
    [HttpPost(ApiRoutes.AccountTransaction.Withdraw)]
    [ProducesResponseType(typeof(SuccessResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(FailureResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(FailureResponse), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Withdraw([FromBody] WithdrawRequest request)
    {
        var result = await accountTransactionService.WithdrawAsync(UserId, request.Amount);
        return result.Match(
            _ => Ok(new SuccessResponse("Withdrawal successful")),
            failure => failure.ToActionResult()
        );
    }

    /// <summary>
    /// Transfers funds from one user's account to another.
    /// </summary>
    /// <response code="200">Transfer successful.</response>
    /// <response code="400">Bad request (e.g. same user or insufficient funds).</response>
    /// <response code="404">One or both users' balances not found.</response>
    [HttpPost(ApiRoutes.AccountTransaction.Transfer)]
    [ProducesResponseType(typeof(SuccessResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(FailureResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(FailureResponse), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Transfer([FromBody] TransferRequest request)
    {
        var result = await accountTransactionService.TransferAsync(UserId, request.ToUserId, request.Amount);
        return result.Match(
            _ => Ok(new SuccessResponse("Transfer successful")),
            failure => failure.ToActionResult()
        );
    }
}
