namespace Contracts.V1.Responses.Account;

public class UserAccountResponse
{
    public required Guid Id { get; set; }
    public required string Email { get; set; }
}
