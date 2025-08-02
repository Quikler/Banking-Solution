using System.Net;
using System.Net.Http.Json;
using Contracts;
using Contracts.V1.Requests.Transactions;
using Contracts.V1.Responses;
using Microsoft.EntityFrameworkCore;
using Shouldly;
using WebApi.IntegrationTests.Abstractions;
using WebApi.IntegrationTests.Extensions;

namespace WebApi.IntegrationTests.Transactions;

public class AccountTransactionsWithdrawTests(IntegrationTestWebApplicationFactory factory) : BaseAccountTransactionsTests(factory)
{
    [Fact]
    public async Task Withdraw_WithSufficientBalance_ReturnsSuccess()
    {
        // Arrange
        var authResponse = await this.SignupRandomAsync();
        this.SetBearerToken(authResponse.Token);

        var depositRequest = new DepositRequest { Amount = 200.00m };
        await HttpClient.PostAsJsonAsync(ApiRoutes.AccountTransaction.Deposit, depositRequest);

        var withdrawRequest = new WithdrawRequest { Amount = 150.00m };

        // Act
        var response = await HttpClient.PostAsJsonAsync(ApiRoutes.AccountTransaction.Withdraw, withdrawRequest);

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.OK);

        var result = await response.Content.ReadFromJsonAsync<SuccessResponse>();
        result.ShouldNotBeNull();
        result.Message.ShouldBe("Withdrawal successful");
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-25)]
    public async Task Withdraw_WithInvalidAmount_ReturnsBadRequest(decimal invalidAmount)
    {
        // Arrange
        var authResponse = await this.SignupRandomAsync();
        this.SetBearerToken(authResponse.Token);

        var withdrawRequest = new WithdrawRequest { Amount = invalidAmount };

        // Act
        var response = await HttpClient.PostAsJsonAsync(ApiRoutes.AccountTransaction.Withdraw, withdrawRequest);

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.BadRequest);

        var result = await response.Content.ReadFromJsonAsync<FailureResponse>();
        result.ShouldNotBeNull();
        result.Errors.ShouldContain("Amount must be greater than zero");
    }

    [Fact]
    public async Task Withdraw_WithInsufficientFunds_ReturnsBadRequest()
    {
        // Arrange
        var authResponse = await this.SignupRandomAsync();
        this.SetBearerToken(authResponse.Token);

        var withdrawRequest = new WithdrawRequest { Amount = 999.99m };

        // Act
        var response = await HttpClient.PostAsJsonAsync(ApiRoutes.AccountTransaction.Withdraw, withdrawRequest);

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.BadRequest);

        var result = await response.Content.ReadFromJsonAsync<FailureResponse>();
        result.ShouldNotBeNull();
        result.Errors.ShouldContain("Not enough funds");
    }

    [Fact]
    public async Task Withdraw_WhenBalanceNotFound_ReturnsNotFound()
    {
        // Arrange
        var authResponse = await this.SignupRandomAsync();
        this.SetBearerToken(authResponse.Token);

        await DbContext.Balances
            .Where(b => b.UserId == authResponse.User.Id)
            .ExecuteDeleteAsync();

        var withdrawRequest = new WithdrawRequest { Amount = 10.00m };

        // Act
        var response = await HttpClient.PostAsJsonAsync(ApiRoutes.AccountTransaction.Withdraw, withdrawRequest);

        response.StatusCode.ShouldBe(HttpStatusCode.NotFound);

        // Assert
        var result = await response.Content.ReadFromJsonAsync<FailureResponse>();
        result.ShouldNotBeNull();
        result.Errors.ShouldContain("Balance for user not found");
    }
}
