using AutoFixture;
using BAL.Services.Account;
using DAL.Entities;
using MockQueryable.Moq;
using Shouldly;

namespace BAL.UnitTests.Account;

public class AccountManagementServiceGetAllAccountsTests : BaseAccountManagementServiceTests
{
    private readonly List<UserEntity> _users;

    public AccountManagementServiceGetAllAccountsTests()
    {
        _users = [.. Fixture.Build<UserEntity>()
            .With(u => u.Balance, () => Fixture.Build<BalanceEntity>().Create())
            .CreateMany(5)];
    }

    [Fact]
    public async Task GetAllAccountsAsync_ShouldReturnAllAccounts_WhenUsersExist()
    {
        // Arrange
        var usersDbSetMock = _users.BuildMockDbSet();
        UserManagerMock
            .Setup(userManager => userManager.Users)
            .Returns(usersDbSetMock.Object);

        // Act
        var result = await AccountManagementService.GetAllAccountsAsync();

        // Assert
        result.IsSuccess.ShouldBeTrue();

        var matchResult = result.Match(
            users => users,
            failure => throw new Exception("Should not be failure.")
        );

        matchResult.Count.ShouldBe(_users.Count);

        foreach (var userDto in matchResult)
        {
            var matchingUser = _users.FirstOrDefault(u => u.Id == userDto.Id);
            matchingUser.ShouldNotBeNull();
        }
    }

    [Fact]
    public async Task GetAllAccountsAsync_ShouldReturnEmptyList_WhenNoUsersExist()
    {
        // Arrange
        _users.Clear();

        var usersDbSetMock = _users.BuildMockDbSet();
        UserManagerMock
            .Setup(userManager => userManager.Users)
            .Returns(usersDbSetMock.Object);

        // Act
        var result = await AccountManagementService.GetAllAccountsAsync();

        // Assert
        result.IsSuccess.ShouldBeTrue();

        var matchResult = result.Match(
            users => users,
            failure => throw new Exception("Should not be failure.")
        );

        matchResult.ShouldBeEmpty();
    }
}
