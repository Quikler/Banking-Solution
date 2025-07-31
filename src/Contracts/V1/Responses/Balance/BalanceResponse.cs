namespace Contracts.V1.Responses.Balance;

public class BalanceResponse
{
    public required Guid Id { get; set; }
    public required Guid UserId { get; set; }
    public required decimal Balance { get; set; }
}
