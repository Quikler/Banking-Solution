using BAL.Providers;
using Common;
using Common.DTOs;
using DAL.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace BAL.Services.Account;

public class AccountManagement(UserManager<UserEntity> userManager, TokenProvider tokenProvider) : IAccontManagement
{
    public async Task<Result<AuthSuccessDto, FailureDto>> LoginAsync(string email, string password)
    {
        var user = await userManager.FindByEmailAsync(email);
        if (user is null)
        {
            return FailureDto.Unauthorized("Invalida email or password");
        }

        var isCorrectPassword = await userManager.CheckPasswordAsync(user, password);
        return isCorrectPassword ? await GenerateAuthDtoForUserAsync(user) : FailureDto.Unauthorized("Invalid email or password");
    }

    public async Task<Result<AuthSuccessDto, FailureDto>> SignupAsync(string email, string password)
    {
        if (await userManager.Users.AnyAsync(u => u.Email == email))
        {
            return FailureDto.Conflict("Email already exist");
        }

        var user = new UserEntity
        {
            UserName = email,
            Email = email,
        };

        // Initial balance
        var balance = new BalanceEntity
        {
            User = user,
            Balance = 0m,
        };

        user.Balance = balance;

        var createResult = await userManager.CreateAsync(user, password);
        return createResult.Succeeded ? await GenerateAuthDtoForUserAsync(user) : FailureDto.BadRequest(createResult.Errors.Select(e => e.Description));
    }

    private async Task<AuthSuccessDto> GenerateAuthDtoForUserAsync(UserEntity user)
    {
        IList<string> roles = await userManager.GetRolesAsync(user);
        return CreateAuthDto(user, tokenProvider.CreateToken(user, roles));
    }

    private static AuthSuccessDto CreateAuthDto(UserEntity user, string token) => new()
    {
        Token = token,
        User = new UserDto
        {
            Id = user.Id,
            Email = user.Email,
        },
    };
}
