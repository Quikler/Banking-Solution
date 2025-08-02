using System.Net.Http.Headers;
using System.Net.Http.Json;
using Bogus;
using Contracts;
using Contracts.V1.Requests.Account;
using Contracts.V1.Responses.Account;
using Shouldly;
using WebApi.IntegrationTests.Abstractions;

namespace WebApi.IntegrationTests.Extensions;

public static class BaseIntegrationTestExtensions
{
    public static async Task<HttpResponseMessage> SingupUserAndGetHttpResponseMessageAsync(this BaseIntegrationTest baseIntegrationTest, string email, string password)
    {
        var signupRequest = new SignupUserRequest
        {
            Email = email,
            Password = password,
            ConfirmPassword = password,
        };

        return await baseIntegrationTest.HttpClient.PostAsJsonAsync(ApiRoutes.Account.Signup, signupRequest);
    }

    public static async Task<AuthResponse> SignupRandomAsync(this BaseIntegrationTest baseIntegrationTest)
    {
        var fakeEmail = new Faker().Internet.Email();
        return await baseIntegrationTest.SignupUserAsync(fakeEmail, Guid.NewGuid().ToString());
    }

    public static async Task<AuthResponse> SignupUserAsync(this BaseIntegrationTest baseIntegrationTest, string email, string password)
    {
        var response = await baseIntegrationTest.SingupUserAndGetHttpResponseMessageAsync(email, password);
        response.StatusCode.ShouldBe(System.Net.HttpStatusCode.OK);
        var data = await response.Content.ReadFromJsonAsync<AuthResponse>();

        data.ShouldNotBeNull();
        return data;
    }

    public static async Task<AuthResponse> LoginUserAsync(this BaseIntegrationTest baseIntegrationTest, string email, string password)
    {
        var loginRequest = new LoginUserRequest
        {
            Email = email,
            Password = password,
        };

        var response = await baseIntegrationTest.HttpClient.PostAsJsonAsync(ApiRoutes.Account.Login, loginRequest);
        response.StatusCode.ShouldBe(System.Net.HttpStatusCode.OK);
        var data = await response.Content.ReadFromJsonAsync<AuthResponse>();

        data.ShouldNotBeNull();
        return data;
    }

    public static async Task SignupAndAuthenticateUserAsync(this BaseIntegrationTest baseIntegrationTest, string email, string password)
    {
        var authResponse = await baseIntegrationTest.SignupUserAsync(email, password);
        baseIntegrationTest.SetBearerToken(authResponse.Token);
    }

    public static async Task LoginAndAuthenticateUserAsync(this BaseIntegrationTest baseIntegrationTest, string email, string password)
    {
        var authResponse = await baseIntegrationTest.LoginUserAsync(email, password);
        baseIntegrationTest.SetBearerToken(authResponse.Token);
    }

    public static void SetBearerToken(this BaseIntegrationTest baseIntegrationTest, string token)
    {
        baseIntegrationTest.HttpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
    }

    public static void LogoutUser(this BaseIntegrationTest baseIntegrationTest)
    {
        baseIntegrationTest.HttpClient.DefaultRequestHeaders.Authorization = null;
    }
}
