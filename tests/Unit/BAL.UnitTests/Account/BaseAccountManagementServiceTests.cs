using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using BAL.Providers;
using BAL.Services.Account;
using Base.UnitTests;
using Common.Configurations;
using DAL;
using DAL.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using Moq;
using Shouldly;

namespace BAL.UnitTests.Account;

public class BaseAccountManagementServiceTests : BaseUnitTests
{
    protected virtual Mock<AppDbContext> DbContextMock { get; }
    protected virtual Mock<UserManager<UserEntity>> UserManagerMock { get; }

    protected virtual TokenProvider TokenProvider { get; }
    protected virtual JwtConfiguration JwtConfiguration { get; }

    protected virtual AccountManagementService AccountManagementService { get; }

    protected virtual TokenValidationParameters TokenValidationParameters { get; }

    protected void ValidateJwt(Guid userId, string token)
    {
        Exception? ex = Record.Exception(() =>
        {
            var principal = new JwtSecurityTokenHandler().ValidateToken(token, TokenValidationParameters, out var validatedToken);
            principal.FindFirst(ClaimTypes.NameIdentifier)?.Value.ShouldBe(userId.ToString());
        });

        ex.ShouldBeNull();
    }

    public BaseAccountManagementServiceTests()
    {
        DbContextMock = new Mock<AppDbContext>();

        var userStoreMock = new Mock<IUserStore<UserEntity>>();
        UserManagerMock = new Mock<UserManager<UserEntity>>(userStoreMock.Object, null!, null!, null!, null!, null!, null!, null!, null!);

        JwtConfiguration = new JwtConfiguration
        {
            SecretKey = ",bGewnAe)0(7./{vwVnBnRK%S*xb08KP",
            ValidIssuer = "test",
            ValidAudience = "test",
            RefreshTokenLifetime = TimeSpan.FromDays(180),
            TokenLifetime = TimeSpan.FromSeconds(45),
        };

        TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(System.Text.Encoding.UTF8.GetBytes(JwtConfiguration.SecretKey)),
            ValidateIssuer = true,
            ValidIssuer = JwtConfiguration.ValidIssuer,
            ValidateAudience = true,
            ValidAudience = JwtConfiguration.ValidAudience,
            ValidateLifetime = true,
            ClockSkew = TimeSpan.Zero
        };

        var jwtConfigurationOptions = Options.Create(JwtConfiguration);

        TokenProvider = new TokenProvider(jwtConfigurationOptions);

        AccountManagementService = new AccountManagementService(UserManagerMock.Object, TokenProvider, DbContextMock.Object, jwtConfigurationOptions);
    }
}
