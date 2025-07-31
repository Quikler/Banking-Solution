namespace Common.DTOs;

public class UserDto
{
    public required Guid Id { get; set; }
    public required string Email { get; set; }
    public required BalanceDto Balance { get; set; }
}
