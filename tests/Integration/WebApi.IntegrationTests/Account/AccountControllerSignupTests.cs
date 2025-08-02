using System.Net.Http.Json;
using Contracts;
using Contracts.V1.Responses.Account;
using Microsoft.EntityFrameworkCore;
using Shouldly;
using WebApi.IntegrationTests.Abstractions;
using WebApi.IntegrationTests.Extensions;

namespace WebApi.IntegrationTests.Account;

public class AccountControllerSignupTests(IntegrationTestWebApplicationFactory factory) : BaseAccountTests(factory)
{
    [Fact]
    public async Task Signup_WhenDataIsValid_ShouldSucceedAndAddUserToDatabase()
    {
        // Arrange
        var signupRequest = CreateSignupRequest(TestEmail, TestPassword, TestPassword);

        // Act
        var response = await HttpClient.PostAsJsonAsync(ApiRoutes.Account.Signup, signupRequest);

        // Assert
        response.StatusCode.ShouldBe(System.Net.HttpStatusCode.OK);
        response.Headers.TryGetValues("Set-Cookie", out var setCookieHeaders);
        setCookieHeaders.ShouldNotBeNull();
        setCookieHeaders.Any(header => header.StartsWith("refreshToken=")).ShouldBeTrue();

        var content = await response.Content.ReadFromJsonAsync<AuthResponse>();

        content.ShouldNotBeNull();
        content.User.Email.ShouldBe(TestEmail);

        var user = await DbContext.Users.FirstOrDefaultAsync(u => u.Email == signupRequest.Email);
        user.ShouldNotBeNull();
    }

    [Fact]
    public async Task Signup_WhenPasswordsDoNotMatch_ShouldFailAndNotAddUserToDatabase()
    {
        // Arrange
        var signupRequest = CreateSignupRequest(TestEmail, TestPassword, "different_password");

        // Act
        var response = await HttpClient.PostAsJsonAsync(ApiRoutes.Account.Signup, signupRequest);

        // Assert
        response.StatusCode.ShouldBe(System.Net.HttpStatusCode.BadRequest);

        var user = await DbContext.Users.FirstOrDefaultAsync(u => u.Email == signupRequest.Email);
        user.ShouldBeNull();
    }

    [Fact]
    public async Task Signup_WhenEmailExist_ShouldFailAndNotAddUserToDatabase()
    {
        // Arrange
        await this.SignupUserAsync(TestEmail, TestPassword);
        var secondSignupRequest = CreateSignupRequest(TestEmail, TestPassword, TestPassword);

        // Act
        var secondSignupResponse = await HttpClient.PostAsJsonAsync(ApiRoutes.Account.Signup, secondSignupRequest);

        // Assert
        secondSignupResponse.StatusCode.ShouldBe(System.Net.HttpStatusCode.Conflict);

        var user = await DbContext.Users.SingleOrDefaultAsync(u => u.Email == secondSignupRequest.Email);
        user.ShouldNotBeNull();
    }

    [Theory]
    [InlineData("")]
    [InlineData("  ")]
    public async Task Signup_WhenEmailIsInvalid_ShouldFailAndNotAddUserToDatabase(string email)
    {
        // Arrange
        var signupRequest = CreateSignupRequest(email, TestPassword, TestPassword);

        // Act
        var response = await HttpClient.PostAsJsonAsync(ApiRoutes.Account.Signup, signupRequest);

        // Assert
        response.StatusCode.ShouldBe(System.Net.HttpStatusCode.BadRequest);

        var user = await DbContext.Users.FirstOrDefaultAsync(u => u.Email == signupRequest.Email);
        user.ShouldBeNull();
    }

    [Theory]
    [InlineData("")]
    [InlineData("  ")]
    [InlineData("short")]
    [InlineData("1234567")]
    public async Task Signup_WhenPasswordIsInvalid_ShouldFailAndNotAddUserToDatabase(string password)
    {
        // Arrange
        var signupRequest = CreateSignupRequest(TestEmail, password, password);

        // Act
        var signupResponse = await HttpClient.PostAsJsonAsync(ApiRoutes.Account.Signup, signupRequest);

        // Assert
        signupResponse.StatusCode.ShouldBe(System.Net.HttpStatusCode.BadRequest);

        var user = await DbContext.Users.FirstOrDefaultAsync(u => u.Email == signupRequest.Email);
        user.ShouldBeNull();
    }
}
