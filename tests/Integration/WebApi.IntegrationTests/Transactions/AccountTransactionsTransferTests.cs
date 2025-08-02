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

public class AccountTransactionsTransferTests(IntegrationTestWebApplicationFactory factory) : BaseAccountTransactionsTests(factory)
{
    [Fact]
    public async Task Transfer_WithValidData_ReturnsSuccess()
    {
        // Arrange
        var sender = await this.SignupRandomAsync();
        this.SetBearerToken(sender.Token);

        var receiver = await this.SignupRandomAsync();

        await HttpClient.PostAsJsonAsync(ApiRoutes.AccountTransaction.Deposit, new DepositRequest { Amount = 300m });

        var transferRequest = new TransferRequest
        {
            ToUserId = receiver.User.Id,
            Amount = 200m
        };

        // Act
        var response = await HttpClient.PostAsJsonAsync(ApiRoutes.AccountTransaction.Transfer, transferRequest);

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.OK);

        var result = await response.Content.ReadFromJsonAsync<SuccessResponse>();
        result.ShouldNotBeNull();
        result.Message.ShouldBe("Transfer successful");
    }

    [Fact]
    public async Task Transfer_ToSelf_ReturnsBadRequest()
    {
        // Arrange
        var user = await this.SignupRandomAsync();
        this.SetBearerToken(user.Token);

        var transferRequest = new TransferRequest
        {
            ToUserId = user.User.Id,
            Amount = 100m
        };

        // Act
        var response = await HttpClient.PostAsJsonAsync(ApiRoutes.AccountTransaction.Transfer, transferRequest);

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.BadRequest);

        var result = await response.Content.ReadFromJsonAsync<FailureResponse>();
        result.ShouldNotBeNull();
        result.Errors.ShouldContain("Cannot transfer funds to the same user");
    }

    [Fact]
    public async Task Transfer_WithInsufficientFunds_ReturnsBadRequest()
    {
        // Arrange
        var sender = await this.SignupRandomAsync();
        this.SetBearerToken(sender.Token);

        var receiver = await this.SignupRandomAsync();

        var transferRequest = new TransferRequest
        {
            ToUserId = receiver.User.Id,
            Amount = 9999m
        };

        // Act
        var response = await HttpClient.PostAsJsonAsync(ApiRoutes.AccountTransaction.Transfer, transferRequest);

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.BadRequest);

        var result = await response.Content.ReadFromJsonAsync<FailureResponse>();
        result.ShouldNotBeNull();
        result.Errors.ShouldContain("Not enough funds for transfer");
    }

    [Fact]
    public async Task Transfer_WhenReceiverBalanceNotFound_ReturnsNotFound()
    {
        // Arrange
        var sender = await this.SignupRandomAsync();
        this.SetBearerToken(sender.Token);

        var receiver = await this.SignupRandomAsync();

        await HttpClient.PostAsJsonAsync(ApiRoutes.AccountTransaction.Deposit, new DepositRequest { Amount = 100m });

        await DbContext.Balances
            .Where(b => b.UserId == receiver.User.Id)
            .ExecuteDeleteAsync();

        var transferRequest = new TransferRequest
        {
            ToUserId = receiver.User.Id,
            Amount = 50m
        };

        // Act
        var response = await HttpClient.PostAsJsonAsync(ApiRoutes.AccountTransaction.Transfer, transferRequest);

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.NotFound);

        var result = await response.Content.ReadFromJsonAsync<FailureResponse>();
        result.ShouldNotBeNull();
        result.Errors.ShouldContain("Balance for recipient user not found");
    }

    [Fact]
    public async Task Transfer_WhenSenderBalanceNotFound_ReturnsNotFound()
    {
        // Arrange
        var sender = await this.SignupRandomAsync();
        this.SetBearerToken(sender.Token);

        var receiver = await this.SignupRandomAsync();

        await DbContext.Balances
            .Where(b => b.UserId == sender.User.Id)
            .ExecuteDeleteAsync();

        var transferRequest = new TransferRequest
        {
            ToUserId = receiver.User.Id,
            Amount = 50m
        };

        // Act
        var response = await HttpClient.PostAsJsonAsync(ApiRoutes.AccountTransaction.Transfer, transferRequest);

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.NotFound);

        var result = await response.Content.ReadFromJsonAsync<FailureResponse>();
        result.ShouldNotBeNull();
        result.Errors.ShouldContain("Balance for sender user not found");
    }
}
