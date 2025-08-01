using Common.DTOs;
using Contracts.V1.Requests.Account;

namespace Mappers.Mapping;

public static class ApiContractToDto
{
    public static LoginUserDto ToDto(this LoginUserRequest request)
    {
        return new LoginUserDto
        {
            Email = request.Email,
            Password = request.Password,
        };
    }

    public static SignupUserDto ToDto(this SignupUserRequest request)
    {
        return new SignupUserDto
        {
            Email = request.Email,
            Password = request.Password,
        };
    }
}
