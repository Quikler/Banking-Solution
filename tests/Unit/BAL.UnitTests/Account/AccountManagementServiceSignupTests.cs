using System.IdentityModel.Tokens.Jwt;
using AutoFixture;
using Base.UnitTests.Extensions;
using Common.DTOs;
using DAL.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using MockQueryable.Moq;
using Moq;
using Shouldly;

namespace BAL.UnitTests.Account;

public class AccountManagementServiceSignupTests : BaseAccountManagementServiceTests
{
    private readonly SignupUserDto _signupUserDto;
    private readonly UserEntity _user;

    public AccountManagementServiceSignupTests()
    {
        _signupUserDto = Fixture.Create<SignupUserDto>();
        _user = Fixture.Build<UserEntity>()
            .With(u => u.Email, _signupUserDto.Email)
            .Create();
    }

    [Fact]
    public async Task SignupAsync_ShouldReturnError_WhenEmailAlreadyRegistered()
    {
        // Arrange
        UserEntity[] users = [_user];

        var usersDbSetMock = users.BuildMockDbSet();
        UserManagerMock
            .Setup(userManager => userManager.Users)
            .Returns(usersDbSetMock.Object);

        // Act
        var signupResult = await AccountManagementService.SignupAsync(_signupUserDto);

        // Assert
        signupResult.IsSuccess.ShouldBeFalse();

        var matchResult = signupResult.Match(
            success => [],
            failure => failure.Errors
        );

        matchResult.ShouldContain("Email already exist");
    }

    [Fact]
    public async Task SignupAsync_ShouldReturnError_WhenCreateAsyncFails()
    {
        // Arrange
        UserEntity[] users = [];

        var usersDbSetMock = users.BuildMockDbSet();
        UserManagerMock
            .Setup(userManager => userManager.Users)
            .Returns(usersDbSetMock.Object);

        UserManagerMock
            .Setup(userManager => userManager.CreateAsync(It.IsAny<UserEntity>(), It.IsAny<string>()))
            .ReturnsAsync(IdentityResult.Failed([new IdentityError
            {
                Code = "SOME_ERROR_CODE",
                Description = "Some error happened.",
            }]));

        // Act
        var signupResult = await AccountManagementService.SignupAsync(_signupUserDto);

        // Assert
        signupResult.IsSuccess.ShouldBeFalse();

        var matchResult = signupResult.Match(
            success => [],
            failure => failure.Errors
        );

        matchResult.ShouldContain("Some error happened.");

        UserManagerMock.VerifyCreateAsync(_signupUserDto.Email, _signupUserDto.Password);
    }

    [Fact]
    public async Task SignupAsync_ShouldReturnEmailConfirmDto_WhenUserDoesntExist()
    {
        // Arrange
        UserEntity[] users = [];

        var usersDbSetMock = users.BuildMockDbSet();
        UserManagerMock
            .Setup(userManager => userManager.Users)
            .Returns(usersDbSetMock.Object);

        UserManagerMock
            .Setup(userManager => userManager.CreateAsync(
                It.IsAny<UserEntity>(),
                It.IsAny<string>()))
            .Callback<UserEntity, string>((user, _) => user.Id = Guid.NewGuid())
            .ReturnsAsync(IdentityResult.Success);

        var refreshTokensDbSetMock = new Mock<DbSet<RefreshTokenEntity>>();
        DbContextMock
            .Setup(dbContext => dbContext.RefreshTokens)
            .Returns(refreshTokensDbSetMock.Object);

        // Act
        var signupResult = await AccountManagementService.SignupAsync(_signupUserDto);

        // Assert
        signupResult.IsSuccess.ShouldBeTrue();

        var matchResult = signupResult.Match(
            success =>
            {
                success.User.ShouldNotBe(default);
                ValidateJwt(success.User.Id, success.Token);
                return true;
            },
            failure => false
        );

        matchResult.ShouldBeTrue();

        UserManagerMock.VerifyCreateAsync(_signupUserDto.Email, _signupUserDto.Password);
    }
}
