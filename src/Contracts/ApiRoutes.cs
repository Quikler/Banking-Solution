namespace Contracts;

public class ApiRoutes
{
    public const string ROOT = "api";
    public const string VERSION = "v1";

    public const string BASE = $"{ROOT}/{VERSION}";

    public static class Account
    {
        public const string Login = BASE + "/accounts/login";
        public const string Signup = BASE + "/accounts/signup";
    }
}
