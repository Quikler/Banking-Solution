namespace Contracts.V1.Responses.Account;

public record AuthResponse
{
    public required string Token { get; set; }
    public required UserResponse User { get; set; }
}
