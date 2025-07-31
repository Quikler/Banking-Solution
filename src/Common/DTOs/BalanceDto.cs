namespace Common.DTOs;

public class BalanceDto
{
    public required Guid Id { get; set; }
    public required Guid UserId { get; set; }
    public required decimal Balance { get; set; }
}
