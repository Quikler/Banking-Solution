using System.Net;
using System.Net.Http.Json;
using Contracts;
using Contracts.V1.Responses.Account;
using Shouldly;
using WebApi.IntegrationTests.Abstractions;
using WebApi.IntegrationTests.Extensions;

namespace WebApi.IntegrationTests.Account;

public class AccountControllerRefreshTests(IntegrationTestWebApplicationFactory factory) : BaseAccountTests(factory)
{
    [Fact]
    public async Task Refresh_WhenRefreshTokenIsValid_ShouldReturnNewAuthTokens()
    {
        // Arrange
        var authResponse = await this.SingupUserAndGetHttpResponseMessageAsync(TestEmail, TestPassword);

        authResponse.Headers.TryGetValues("Set-Cookie", out var setCookieHeaders);
        setCookieHeaders.ShouldNotBeNull();

        var refreshToken = setCookieHeaders
            .FirstOrDefault(header => header.StartsWith("refreshToken="))?
            .Split(';')[0]
            .Split('=')[1];

        var nextRequest = new HttpRequestMessage(HttpMethod.Post, ApiRoutes.Account.Refresh);
        nextRequest.Headers.Add("Cookie", $"refreshToken={refreshToken}");
        Console.WriteLine($"Bebra: {refreshToken}");

        // Act
        var response = await HttpClient.SendAsync(nextRequest);

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.OK);

        var content = await response.Content.ReadFromJsonAsync<AuthResponse>();
        content.ShouldNotBeNull();
        content.User.ShouldNotBeNull();
        content.Token.ShouldNotBeNullOrWhiteSpace();

        response.Headers.TryGetValues("Set-Cookie", out var setCookieHeaders2);
        setCookieHeaders.ShouldNotBeNull();
        setCookieHeaders.Any(header => header.StartsWith("refreshToken=")).ShouldBeTrue();
    }

    [Fact]
    public async Task Refresh_WhenRefreshTokenIsMissing_ShouldReturnUnauthorized()
    {
        // Arrange
        var request = new HttpRequestMessage(HttpMethod.Post, ApiRoutes.Account.Refresh);

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task Refresh_WhenRefreshTokenIsInvalid_ShouldReturnUnauthorized()
    {
        // Arrange
        var request = new HttpRequestMessage(HttpMethod.Post, ApiRoutes.Account.Refresh);
        request.Headers.Add("Cookie", "refreshToken=invalid_or_expired_token");

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.Unauthorized);
    }
}
