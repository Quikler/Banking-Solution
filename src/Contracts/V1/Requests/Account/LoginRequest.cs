namespace Contracts.V1.Requests.Account;

public record LoginRequest
{
    public required string Email { get; set; }
    public required string Password { get; set; }
}
