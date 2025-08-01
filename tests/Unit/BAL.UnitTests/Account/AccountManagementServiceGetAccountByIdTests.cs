using AutoFixture;
using BAL.Services.Account;
using DAL.Entities;
using MockQueryable.Moq;
using Shouldly;

namespace BAL.UnitTests.Account;

public class AccountManagementServiceGetAccountByIdTests : BaseAccountManagementServiceTests
{
    private readonly UserEntity _user;

    public AccountManagementServiceGetAccountByIdTests()
    {
        _user = Fixture.Build<UserEntity>().Create();
    }

    [Fact]
    public async Task GetAccountById_ShouldReturnError_WhenUserNotFound()
    {
        // Arrange
        UserEntity[] users = [];

        var usersDbSetMock = users.BuildMockDbSet();
        UserManagerMock
            .Setup(userManager => userManager.Users)
            .Returns(usersDbSetMock.Object);

        // Act
        var meResult = await AccountManagementService.GetAccountByIdAsync(Guid.NewGuid());

        // Assert
        meResult.IsSuccess.ShouldBeFalse();

        var matchResult = meResult.Match(
            authSuccesDto => [],
            failure => failure.Errors
        );

        matchResult.ShouldContain("User not found");
    }

    [Fact]
    public async Task MeAsync_ShouldReturnAuthSuccessDto_WhenEveryCheckPasses()
    {
        // Arrange
        UserEntity[] users = [_user];

        var usersDbSetMock = users.BuildMockDbSet();
        UserManagerMock
            .Setup(userManager => userManager.Users)
            .Returns(usersDbSetMock.Object);

        // Act
        var refreshResult = await AccountManagementService.GetAccountByIdAsync(_user.Id);

        // Assert
        refreshResult.IsSuccess.ShouldBeTrue();

        var matchResult = refreshResult.Match(
            authSuccessDto => authSuccessDto,
            failure => throw new Exception("Should not be failure.")
        );

        matchResult.Id.ShouldBe(_user.Id);
        matchResult.Balance.Balance.ShouldBe(_user.Balance.Balance);
    }
}
