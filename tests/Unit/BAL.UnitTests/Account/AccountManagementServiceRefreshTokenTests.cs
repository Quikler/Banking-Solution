using AutoFixture;
using DAL.Entities;
using MockQueryable.Moq;
using Shouldly;

namespace BAL.UnitTests.Account;

public class AccountManagementServiceRefreshTokenTests : BaseAccountManagementServiceTests
{
    private readonly RefreshTokenEntity _refreshTokenEntity;
    private readonly UserEntity _user;

    public AccountManagementServiceRefreshTokenTests()
    {
        _user = Fixture.Create<UserEntity>();

        _refreshTokenEntity = Fixture.Build<RefreshTokenEntity>()
            .With(r => r.User, _user)
            .With(r => r.UserId, _user.Id)
            .With(r => r.Token, TokenProvider.GenerateRefreshToken())
            .Create();
    }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public async Task RefreshTokenAsync_ShouldReturnError_WhenTokenNotFoundOrExpired(bool isTokenFound)
    {
        // Arrange
        List<RefreshTokenEntity> refreshTokens = [];
        if (isTokenFound)
        {
            _refreshTokenEntity.ExpiryDate = DateTime.UtcNow - TimeSpan.FromDays(1);
            refreshTokens.Add(_refreshTokenEntity);
        }

        var refreshTokensDbSetMock = refreshTokens.BuildMockDbSet();

        DbContextMock
            .Setup(dbContext => dbContext.RefreshTokens)
            .Returns(refreshTokensDbSetMock.Object);

        // Act
        var refreshResult = await AccountManagementService.RefreshTokenAsync(_refreshTokenEntity.Token);

        // Assert
        refreshResult.IsSuccess.ShouldBeFalse();

        var matchResult = refreshResult.Match(
            authSuccesDto => [],
            failure => failure.Errors
        );

        matchResult.ShouldContain("Refresh token has expired.");
    }

    [Fact]
    public async Task RefreshTokenAsync_ShouldReturnAuthSuccessDto_WhenEveryCheckPasses()
    {
        // Arrange
        var refreshToken = TokenProvider.GenerateRefreshToken();
        _refreshTokenEntity.ExpiryDate = DateTime.UtcNow.Add(JwtConfiguration.RefreshTokenLifetime);
        _refreshTokenEntity.Token = refreshToken;
        List<RefreshTokenEntity> refreshTokens = [_refreshTokenEntity,];

        var refreshTokensDbSetMock = refreshTokens.BuildMockDbSet();

        DbContextMock
            .Setup(dbContext => dbContext.RefreshTokens)
            .Returns(refreshTokensDbSetMock.Object);

        // Act
        var refreshResult = await AccountManagementService.RefreshTokenAsync(refreshToken);

        // Assert
        refreshResult.IsSuccess.ShouldBeTrue();

        var matchResult = refreshResult.Match(
            authSuccessDto => authSuccessDto,
            failure => throw new Exception("Should not be failure.")
        );

        matchResult.User.Id.ShouldBe(_user.Id);
        matchResult.Token.ShouldNotBeNullOrWhiteSpace();
        matchResult.RefreshToken.ShouldNotBe(refreshToken);
    }
}
