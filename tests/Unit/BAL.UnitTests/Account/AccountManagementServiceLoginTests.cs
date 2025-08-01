using AutoFixture;
using Base.UnitTests.Extensions;
using Common.DTOs;
using DAL.Entities;
using Microsoft.EntityFrameworkCore;
using MockQueryable.Moq;
using Moq;
using Shouldly;

namespace BAL.UnitTests.Account;

public class AccountManagementServiceLoginTests : BaseAccountManagementServiceTests
{
    private readonly UserEntity _user;
    private readonly LoginUserDto _loginUserDto;

    public AccountManagementServiceLoginTests()
    {
        _loginUserDto = Fixture.Create<LoginUserDto>();
        _user = Fixture.Build<UserEntity>()
            .With(u => u.Email, _loginUserDto.Email)
            .Create();
    }

    [Fact]
    public async Task LoginAsync_ShouldReturnError_WhenUserNotFound()
    {
        // Arrange
        UserEntity[] users = [];

        var usersDbSetMock = users.BuildMockDbSet();
        UserManagerMock
            .Setup(userManager => userManager.Users)
            .Returns(usersDbSetMock.Object);

        // Act
        var loginResult = await AccountManagementService.LoginAsync(_loginUserDto);

        // Assert
        loginResult.IsSuccess.ShouldBeFalse();

        var matchResult = loginResult.Match(
            success => [],
            failure => failure.Errors
        );

        matchResult.ShouldContain("Invalid email or password");

    }

    [Fact]
    public async Task LoginAsync_ShouldReturnError_WhenPasswordIsInvalid()
    {
        // Arrange
        UserManagerMock.SetupCheckPasswordAsync(false);

        UserEntity[] users = [_user];
        var usersDbSetMock = users.BuildMockDbSet();

        UserManagerMock
            .Setup(userManager => userManager.Users)
            .Returns(usersDbSetMock.Object);

        // Act
        var loginResult = await AccountManagementService.LoginAsync(_loginUserDto);

        // Assert
        loginResult.IsSuccess.ShouldBeFalse();

        var matchResult = loginResult.Match(
            success => [],
            failure => failure.Errors
        );

        matchResult.ShouldContain("Invalid email or password");

        UserManagerMock.VerifyCheckPasswordAsync(_user, _loginUserDto.Password);
    }

    [Fact]
    public async Task LoginAsync_ShouldReturnGenerateSuccessDtoForUser_WhenEveryCheckPasses()
    {
        // Arrange
        UserManagerMock.SetupCheckPasswordAsync(true);

        UserEntity[] users = [_user];

        var usersDbSetMock = users.BuildMockDbSet();
        UserManagerMock
            .Setup(userManager => userManager.Users)
            .Returns(usersDbSetMock.Object);

        var refreshTokensDbSetMock = new Mock<DbSet<RefreshTokenEntity>>();
        DbContextMock
            .Setup(dbContext => dbContext.RefreshTokens)
            .Returns(refreshTokensDbSetMock.Object);

        // Act
        var loginResult = await AccountManagementService.LoginAsync(_loginUserDto);

        // Assert
        loginResult.IsSuccess.ShouldBeTrue();

        var matchResult = loginResult.Match(
            authSuccessDto => authSuccessDto,
            failure => throw new Exception("Should not be failure")
        );

        matchResult.User.Id.ShouldBe(_user.Id);
        matchResult.Token.ShouldNotBeNullOrWhiteSpace();
        matchResult.RefreshToken.ShouldNotBeNullOrWhiteSpace();

        UserManagerMock.VerifyCheckPasswordAsync(_user, _loginUserDto.Password);
    }
}
