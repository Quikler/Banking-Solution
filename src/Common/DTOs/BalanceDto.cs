namespace Common.DTOs;

public class BalanceDto
{
    public required Guid UserId { get; set; }
    public required decimal Balance { get; set; }
}
