using System.Net;
using System.Net.Http.Json;
using Contracts;
using Contracts.V1.Responses.Account;
using Microsoft.EntityFrameworkCore;
using Shouldly;
using WebApi.IntegrationTests.Abstractions;
using WebApi.IntegrationTests.Extensions;

namespace WebApi.IntegrationTests.Account;

public class AccountControllerLoginTests(IntegrationTestWebApplicationFactory factory) : BaseAccountTests(factory)
{
    [Fact]
    public async Task Login_WhenCredentialsAreValid_ShouldSucceedAndReturnUserInfo()
    {
        // Arrange
        await this.SignupUserAsync(TestEmail, TestPassword);
        var loginRequest = CreateLoginRequest(TestEmail, TestPassword);

        // Act
        var response = await HttpClient.PostAsJsonAsync(ApiRoutes.Account.Login, loginRequest);

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.OK);

        response.Headers.TryGetValues("Set-Cookie", out var setCookieHeaders);
        setCookieHeaders.ShouldNotBeNull();
        setCookieHeaders.Any(header => header.StartsWith("refreshToken=")).ShouldBeTrue();

        var content = await response.Content.ReadFromJsonAsync<AuthResponse>();
        content.ShouldNotBeNull();
        content.User.Email.ShouldBe(TestEmail);

        var user = await DbContext.Users.FirstOrDefaultAsync(u => u.Email == loginRequest.Email);
        user.ShouldNotBeNull();
    }

    [Theory]
    [InlineData("wrong@email.com", TestPassword)]
    [InlineData(TestEmail, "wrongpassword")]
    [InlineData("wrong@email.com", "wrongpassword")]
    public async Task Login_WhenCredentialsAreInvalid_ShouldFailWithUnauthorized(string email, string password)
    {
        // Arrange
        await this.SignupUserAsync(TestEmail, TestPassword);
        var loginRequest = CreateLoginRequest(email, password);

        // Act
        var response = await HttpClient.PostAsJsonAsync(ApiRoutes.Account.Login, loginRequest);

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.Unauthorized);
    }

    [Theory]
    [InlineData("", TestPassword)]
    [InlineData("   ", TestPassword)]
    [InlineData(TestEmail, "")]
    [InlineData(TestEmail, "  ")]
    public async Task Login_WhenCredentialsAreMalformed_ShouldFailWithBadRequest(string email, string password)
    {
        // Arrange
        var loginRequest = CreateLoginRequest(email, password);

        // Act
        var response = await HttpClient.PostAsJsonAsync(ApiRoutes.Account.Login, loginRequest);

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.BadRequest);

        var user = await DbContext.Users.FirstOrDefaultAsync(u => u.Email == loginRequest.Email);
        user.ShouldBeNull();
    }
}
