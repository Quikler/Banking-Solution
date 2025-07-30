namespace Common.DTOs;

public class AuthSuccessDto
{
    public required string Token { get; set; }
    public required UserDto User { get; set; }
}
