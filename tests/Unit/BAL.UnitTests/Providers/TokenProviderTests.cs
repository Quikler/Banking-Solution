using BAL.Providers;
using Common.Configurations;
using DAL.Entities;
using Microsoft.Extensions.Options;
using System.Security.Claims;
using System.IdentityModel.Tokens.Jwt;
using Shouldly;

namespace BAL.UnitTests.Providers;

public class TokenProviderTests
{
    private readonly TokenProvider _tokenProvider;

    public TokenProviderTests()
    {
        var config = new JwtConfiguration
        {
            SecretKey = "kh-sGyKNQ3PheFwRpk42swSOOwQm0DliQvELcHaGpxk",
            ValidIssuer = "TestIssuer",
            ValidAudience = "TestAudience",
            TokenLifetime = TimeSpan.FromMinutes(5),
            RefreshTokenLifetime = TimeSpan.FromDays(180),
        };

        _tokenProvider = new TokenProvider(Options.Create(config));
    }

    [Fact]
    public void CreateToken_WithUserAndRoles_ReturnsValidToken()
    {
        // Arrange
        var user = new UserEntity
        {
            Id = Guid.NewGuid(),
            Email = "test@example.com",
            UserName = "testuser"
        };

        var roles = new[] { "Admin", "User" };

        // Act
        var token = _tokenProvider.CreateToken(user, roles);

        // Assert
        token.ShouldNotBeNull();

        var handler = new JwtSecurityTokenHandler();
        handler.CanReadToken(token).ShouldBeTrue();

        var jwt = handler.ReadJwtToken(token);
        jwt.Claims.ShouldContain(c => c.Type == ClaimTypes.NameIdentifier && c.Value == user.Id.ToString());
        jwt.Claims.ShouldContain(c => c.Type == ClaimTypes.Role && c.Value == "Admin");
        jwt.Claims.ShouldContain(c => c.Type == ClaimTypes.Role && c.Value == "User");

        jwt.Claims.ShouldContain(c => c.Type == JwtRegisteredClaimNames.Email && c.Value == user.Email);
        jwt.Claims.ShouldContain(c => c.Type == JwtRegisteredClaimNames.Sub && c.Value == user.UserName);
        jwt.Claims.ShouldContain(c => c.Type == JwtRegisteredClaimNames.Jti && !string.IsNullOrEmpty(c.Value));
    }

    [Fact]
    public void GenerateRefreshToken_ReturnsBase64String()
    {
        // Act
        var refreshToken = _tokenProvider.GenerateRefreshToken();

        // Assert
        refreshToken.ShouldNotBeNull();
        var bytes = Convert.FromBase64String(refreshToken); // should not throw
        bytes.Length.ShouldBe(32);
    }
}
