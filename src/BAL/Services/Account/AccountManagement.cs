using BAL.Providers;
using Common;
using Common.Configurations;
using Common.DTOs;
using DAL;
using DAL.Entities;
using Mappers.Mapping;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace BAL.Services.Account;

public class AccountManagement(UserManager<UserEntity> userManager, TokenProvider tokenProvider, AppDbContext dbContext, IOptions<JwtConfiguration> jwtConfigurationOptions) : IAccontManagement
{
    private readonly JwtConfiguration _jwtConfiguration = jwtConfigurationOptions.Value;

    public async Task<Result<AuthSuccessDto, FailureDto>> LoginAsync(string email, string password)
    {
        var user = await userManager.Users
            .Include(u => u.Balance) // Include to retrieve Balance (maybe better add Lazy loading later)
            .FirstOrDefaultAsync(u => u.Email == email);

        if (user is null)
        {
            return FailureDto.Unauthorized("Invalid email or password");
        }

        var isCorrectPassword = await userManager.CheckPasswordAsync(user, password);
        return isCorrectPassword ? await GenerateAuthSuccessDtoForUserAsync(user) : FailureDto.Unauthorized("Invalid email or password");
    }

    public async Task<Result<AuthSuccessDto, FailureDto>> SignupAsync(string email, string password)
    {
        if (await userManager.Users.AnyAsync(u => u.Email == email))
        {
            return FailureDto.Conflict("Email already exist");
        }

        var user = new UserEntity
        {
            Id = Guid.NewGuid(),
            UserName = email,
            Email = email,
        };

        // Initial balance
        var balance = new BalanceEntity
        {
            UserId = user.Id,
            User = user,
            Balance = 0m,
        };

        user.Balance = balance;

        var createResult = await userManager.CreateAsync(user, password);
        return createResult.Succeeded ? await GenerateAuthSuccessDtoForUserAsync(user) : FailureDto.BadRequest(createResult.Errors.Select(e => e.Description));
    }

    public async Task<Result<AuthSuccessDto, FailureDto>> RefreshTokenAsync(string refreshToken)
    {
        var storedRefreshToken = await dbContext.RefreshTokens
            .Include(r => r.User)
                .ThenInclude(u => u.Balance)
            .FirstOrDefaultAsync(r => r.Token == refreshToken);

        if (!IsRefreshTokenValid(storedRefreshToken))
        {
            return FailureDto.Unauthorized("Refresh token has expired.");
        }

        var newRefreshToken = TokenProvider.GenerateRefreshToken();
        storedRefreshToken!.Token = newRefreshToken;
        storedRefreshToken.ExpiryDate = DateTime.UtcNow.Add(_jwtConfiguration.RefreshTokenLifetime);

        await dbContext.SaveChangesAsync();

        var roles = await userManager.GetRolesAsync(storedRefreshToken.User);
        var token = tokenProvider.CreateToken(storedRefreshToken.User, roles);

        return CreateAuthSuccessDto(storedRefreshToken.User, newRefreshToken, token);
    }

    public async Task<Result<UserDto, FailureDto>> GetAccountByIdAsync(Guid id)
    {
        var user = await userManager.Users
            .Include(u => u.Balance)
            .FirstOrDefaultAsync(u => u.Id == id);

        if (user is null)
        {
            return FailureDto.NotFound("User not found");
        }

        return user.ToUserDto();
    }

    public async Task<Result<List<UserAccountDto>, FailureDto>> GetAllAccountsAsync()
    {
        var users = await userManager.Users
            .Include(u => u.Balance)
            .ToListAsync();

        return users.Select(u => u.ToUserAccountDto()).ToList();
    }

    private async Task<AuthSuccessDto> GenerateAuthSuccessDtoForUserAsync(UserEntity user)
    {
        var roles = await userManager.GetRolesAsync(user);

        var refreshToken = new RefreshTokenEntity
        {
            UserId = user.Id,
            Token = TokenProvider.GenerateRefreshToken(),
            ExpiryDate = DateTime.UtcNow.Add(_jwtConfiguration.RefreshTokenLifetime),
        };

        await dbContext.RefreshTokens.AddAsync(refreshToken);
        await dbContext.SaveChangesAsync();

        return CreateAuthSuccessDto(user, refreshToken.Token, tokenProvider.CreateToken(user, roles));
    }

    private static AuthSuccessDto CreateAuthSuccessDto(UserEntity user, string refreshToken, string token) => new()
    {
        RefreshToken = refreshToken,
        Token = token,
        User = user.ToUserDto(),
    };

    private static bool IsRefreshTokenValid(RefreshTokenEntity? refreshTokenEntity)
    {
        return refreshTokenEntity is not null && refreshTokenEntity.ExpiryDate >= DateTime.UtcNow;
    }
}
