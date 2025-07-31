namespace Contracts.V1.Responses.Account;

public record UserResponse
{
    public required Guid Id { get; set; }
    public required string Email { get; set; }
}
