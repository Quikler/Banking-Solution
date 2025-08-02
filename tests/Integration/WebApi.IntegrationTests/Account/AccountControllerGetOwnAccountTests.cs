using System.Net;
using System.Net.Http.Json;
using Contracts;
using Contracts.V1.Responses.Account;
using Shouldly;
using WebApi.IntegrationTests.Abstractions;
using WebApi.IntegrationTests.Extensions;

namespace WebApi.IntegrationTests.Account;

public class AccountControllerGetOwnAccountTests(IntegrationTestWebApplicationFactory factory) : BaseAccountTests(factory)
{
    [Fact]
    public async Task GetOwnAccount_WhenUserIsAuthenticated_ShouldReturnUserInfo()
    {
        // Arrange
        var authResponse = await this.SignupUserAsync(TestEmail, TestPassword);
        this.SetBearerToken(authResponse.Token);

        // Act
        var response = await HttpClient.GetAsync(ApiRoutes.Account.Me);

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.OK);

        var content = await response.Content.ReadFromJsonAsync<UserResponse>();
        content.ShouldNotBeNull();
        content.Id.ShouldBe(authResponse.User.Id);
        content.Email.ShouldBe(TestEmail);
    }

    [Fact]
    public async Task GetOwnAccount_WhenUserIsNotAuthenticated_ShouldReturnUnauthorized()
    {
        // Arrange
        this.LogoutUser(); // Make sure no token is set

        // Act
        var response = await HttpClient.GetAsync(ApiRoutes.Account.Me);

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task GetOwnAccount_WhenUserIsDeleted_ShouldReturnNotFound()
    {
        // Arrange
        var authResponse = await this.SignupUserAsync(TestEmail, TestPassword);
        this.SetBearerToken(authResponse.Token);

        // Simulate user deletion (assuming you allow direct DB manipulation in tests)
        var user = await DbContext.Users.FindAsync(authResponse.User.Id);
        DbContext.Users.Remove(user!);
        await DbContext.SaveChangesAsync();

        // Act
        var response = await HttpClient.GetAsync(ApiRoutes.Account.Me);

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.NotFound);
    }
}
