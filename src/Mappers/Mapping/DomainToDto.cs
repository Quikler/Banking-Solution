using Common.DTOs;
using DAL.Entities;

namespace Mappers.Mapping;

public static class DomainToDto
{
    public static UserDto ToDto(this UserEntity entity)
    {
        return new UserDto
        {
            Balance = entity.Balance.ToDto(),
            Id = entity.Id,
            Email = entity.Email,
        };
    }

    public static BalanceDto ToDto(this BalanceEntity entity)
    {
        return new BalanceDto
        {
            Id = entity.Id,
            UserId = entity.UserId,
            Balance = entity.Balance,
        };
    }
}
