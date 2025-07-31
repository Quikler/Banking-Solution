using Common.DTOs;
using Contracts.V1.Responses;
using Contracts.V1.Responses.Account;
using Contracts.V1.Responses.Balance;

namespace Mappers.Mapping;

public static class DtoToApiContract
{
    public static FailureResponse ToResponse(this FailureDto failureDto) => new(failureDto.Errors);

    public static AuthResponse ToResponse(this AuthSuccessDto authDto)
    {
        return new AuthResponse
        {
            Token = authDto.Token,
            User = authDto.User.ToResponse(),
        };
    }

    public static UserResponse ToResponse(this UserDto userDto)
    {
        return new UserResponse
        {
            Id = userDto.Id,
            Email = userDto.Email,
            Balance = userDto.Balance.ToResponse(),
        };
    }

    public static UserAccountResponse ToResponse(this UserAccountDto userAccountDto)
    {
        return new UserAccountResponse
        {
            Id = userAccountDto.Id,
            Email = userAccountDto.Email,
        };
    }

    public static BalanceResponse ToResponse(this BalanceDto balanceDto)
    {
        return new BalanceResponse
        {
            Id = balanceDto.Id,
            UserId = balanceDto.UserId,
            Balance = balanceDto.Balance,
        };
    }
}
