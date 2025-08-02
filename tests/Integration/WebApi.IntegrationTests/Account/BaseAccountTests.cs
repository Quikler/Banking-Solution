using Contracts.V1.Requests.Account;
using WebApi.IntegrationTests.Abstractions;

namespace WebApi.IntegrationTests.Account;

public class BaseAccountTests(IntegrationTestWebApplicationFactory factory) : BaseIntegrationTest(factory)
{
    protected const string TestEmail = "this-email-is-only-for-testing-purposes@test.com";
    protected const string TestPassword = "testtest";

    protected static LoginUserRequest CreateLoginRequest(string email, string password) => new()
    {
        Email = email,
        Password = password,
    };

    protected static SignupUserRequest CreateSignupRequest(string email, string password, string confirmPassword) => new()
    {
        Email = email,
        Password = password,
        ConfirmPassword = confirmPassword,
    };
}
