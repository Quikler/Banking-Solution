using Common.DTOs;
using Contracts.V1.Responses;
using Contracts.V1.Responses.Account;

namespace WebApi.Mapping;

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
        };
    }
}
