namespace Contracts.V1.Requests.Account;

public record LoginUserRequest
{
    public required string Email { get; set; }
    public required string Password { get; set; }
}
