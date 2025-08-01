using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using BAL.Services.Account;
using Base.UnitTests;
using Common.Configurations;
using DAL;
using Microsoft.IdentityModel.Tokens;
using Moq;
using Shouldly;

namespace BAL.UnitTests.Transactions;

public class BaseAccountTransactionTests : BaseUnitTests
{
    protected virtual Mock<AppDbContext> DbContextMock { get; }
    protected virtual TokenValidationParameters TokenValidationParameters { get; }

    protected virtual AccountTransactionService AccountTransactionService { get; }

    protected void ValidateJwt(Guid userId, string token)
    {
        Exception? ex = Record.Exception(() =>
        {
            var principal = new JwtSecurityTokenHandler().ValidateToken(token, TokenValidationParameters, out var validatedToken);
            principal.FindFirst(ClaimTypes.NameIdentifier)?.Value.ShouldBe(userId.ToString());
        });

        ex.ShouldBeNull();
    }

    public BaseAccountTransactionTests()
    {
        DbContextMock = new Mock<AppDbContext>();

        var jwtConfiguration = new JwtConfiguration
        {
            SecretKey = ",bGewnAe)0(7./{vwVnBnRK%S*xb08KP",
            ValidIssuer = "test",
            ValidAudience = "test",
            RefreshTokenLifetime = TimeSpan.FromDays(180),
            TokenLifetime = TimeSpan.FromSeconds(45),
        };

        AccountTransactionService = new AccountTransactionService(DbContextMock.Object);

        TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(System.Text.Encoding.UTF8.GetBytes(jwtConfiguration.SecretKey)),
            ValidateIssuer = true,
            ValidIssuer = jwtConfiguration.ValidIssuer,
            ValidateAudience = true,
            ValidAudience = jwtConfiguration.ValidAudience,
            ValidateLifetime = true,
            ClockSkew = TimeSpan.Zero
        };
    }
}
