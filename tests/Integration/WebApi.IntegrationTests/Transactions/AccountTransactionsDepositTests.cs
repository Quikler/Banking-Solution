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

public class AccountTransactionsDepositTests(IntegrationTestWebApplicationFactory factory) : BaseAccountTransactionsTests(factory)
{
    [Fact]
    public async Task Deposit_WithValidAmount_ReturnsSuccess()
    {
        // Arrange
        var authResponse = await this.SignupRandomAsync();
        this.SetBearerToken(authResponse.Token);

        var depositRequest = new DepositRequest { Amount = 100.00m };

        // Act
        var response = await HttpClient.PostAsJsonAsync(ApiRoutes.AccountTransaction.Deposit, depositRequest);

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.OK);

        var result = await response.Content.ReadFromJsonAsync<SuccessResponse>();
        result.ShouldNotBeNull();
        result.Message.ShouldBe("Deposit successful");
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-50)]
    public async Task Deposit_WithInvalidAmount_ReturnsBadRequest(decimal invalidAmount)
    {
        // Arrange
        var authResponse = await this.SignupRandomAsync();
        this.SetBearerToken(authResponse.Token);

        var depositRequest = new DepositRequest { Amount = invalidAmount };

        // Act
        var response = await HttpClient.PostAsJsonAsync(ApiRoutes.AccountTransaction.Deposit, depositRequest);

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.BadRequest);

        var result = await response.Content.ReadFromJsonAsync<FailureResponse>();
        result.ShouldNotBeNull();
        result.Errors.ShouldContain("Amount must be greater than zero");
    }

    [Fact]
    public async Task Deposit_WhenBalanceNotFound_ReturnsNotFound()
    {
        // Arrange
        var authResponse = await this.SignupRandomAsync();
        this.SetBearerToken(authResponse.Token);

        await DbContext.Balances
            .Where(b => b.UserId == authResponse.User.Id)
            .ExecuteDeleteAsync();

        var depositRequest = new DepositRequest { Amount = 50.00m };

        // Act
        var response = await HttpClient.PostAsJsonAsync(ApiRoutes.AccountTransaction.Deposit, depositRequest);

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.NotFound);

        var result = await response.Content.ReadFromJsonAsync<FailureResponse>();
        result.ShouldNotBeNull();
        result.Errors.ShouldContain("Balance for user not found");
    }
}
