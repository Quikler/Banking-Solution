using System.Net;
using System.Net.Http.Json;
using Contracts;
using Contracts.V1.Responses.Account;
using Microsoft.EntityFrameworkCore;
using Shouldly;
using WebApi.IntegrationTests.Abstractions;
using WebApi.IntegrationTests.Extensions;

namespace WebApi.IntegrationTests.Account;


public class AccountControllerGetAccountTests(IntegrationTestWebApplicationFactory factory) : BaseAccountTests(factory)
{
    [Fact]
    public async Task GetAccount_WhenUserIsAuthenticatedAndOwnsAccount_ShouldReturnUserInfo()
    {
        // Arrange
        var authResponse = await this.SignupUserAsync(TestEmail, TestPassword);
        this.SetBearerToken(authResponse.Token);

        var requestUrl = ApiRoutes.Account.GetAccount.Replace("{accountId}", authResponse.User.Id.ToString());

        // Act
        var response = await HttpClient.GetAsync(requestUrl);

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.OK);

        var content = await response.Content.ReadFromJsonAsync<UserResponse>();
        content.ShouldNotBeNull();
        content.Id.ShouldBe(authResponse.User.Id);
        content.Email.ShouldBe(TestEmail);
    }

    [Fact]
    public async Task GetAccount_WhenUserTriesToAccessAnotherAccount_ShouldReturnForbidden()
    {
        // Arrange
        var user1 = await this.SignupUserAsync(TestEmail, TestPassword);
        var user2 = await this.SignupUserAsync("another@test.com", TestPassword);

        this.SetBearerToken(user1.Token);
        var requestUrl = ApiRoutes.Account.GetAccount.Replace("{accountId}", user2.User.Id.ToString());

        // Act
        var response = await HttpClient.GetAsync(requestUrl);

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task GetAccount_WhenUserDoesNotExist_ShouldReturnNotFound()
    {
        // Arrange
        // Signup user to generate jwt
        var authResponse = await this.SignupUserAsync(TestEmail, TestPassword);
        this.SetBearerToken(authResponse.Token);

        // And then delete it
        await DbContext.Users
            .Where(u => u.Id == authResponse.User.Id)
            .ExecuteDeleteAsync();

        var requestUrl = ApiRoutes.Account.GetAccount.Replace("{accountId}", authResponse.User.Id.ToString());

        // Act
        var response = await HttpClient.GetAsync(requestUrl);

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.NotFound);
    }
}
