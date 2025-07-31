using Contracts.V1.Responses.Balance;

namespace Contracts.V1.Responses.Account;

public record UserResponse
{
    public required Guid Id { get; set; }
    public required string Email { get; set; }
    public required BalanceResponse Balance { get; set; }
}
