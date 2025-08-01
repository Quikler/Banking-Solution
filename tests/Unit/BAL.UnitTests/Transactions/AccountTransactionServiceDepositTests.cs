using AutoFixture;
using BAL.Services.Account;
using DAL.Entities;
using MockQueryable.Moq;
using Moq;
using Shouldly;

namespace BAL.UnitTests.Transactions;

public class AccountTransactionServiceDepositTests : BaseAccountTransactionTests
{
    private readonly Guid _userId;
    private readonly UserEntity _user;
    private readonly BalanceEntity _balance;

    public AccountTransactionServiceDepositTests()
    {
        _userId = Guid.NewGuid();

        _user = Fixture.Build<UserEntity>()
            .With(u => u.Id, _userId)
            .Create();

        _balance = Fixture.Build<BalanceEntity>()
          .With(b => b.User, _user)
          .With(b => b.UserId, _userId)
          .Create();
    }

    [Fact]
    public async Task DepositAsync_ShouldReturnNotFound_WhenBalanceDoesNotExist()
    {
        // Arrange
        List<BalanceEntity> balances = [];
        var balancesDbSetMock = balances.BuildMockDbSet();

        DbContextMock
            .Setup(db => db.Balances)
            .Returns(balancesDbSetMock.Object);

        // Act
        var result = await AccountTransactionService.DepositAsync(_userId, 100);

        // Assert
        result.IsSuccess.ShouldBeFalse();
        var matchResult = result.Match(
            _ => throw new Exception("Expected failure"),
            failure => failure);

        matchResult.Errors.ShouldContain("Balance for user not found");
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-50)]
    public async Task DepositAsync_ShouldReturnBadRequest_WhenAmountIsNotPositive(decimal amount)
    {
        // Arrange
        List<BalanceEntity> balances = [_balance];
        var balancesDbSetMock = balances.BuildMockDbSet();

        DbContextMock.Setup(db => db.Balances).Returns(balancesDbSetMock.Object);

        // Act
        var result = await AccountTransactionService.DepositAsync(_userId, amount);

        // Assert
        result.IsSuccess.ShouldBeFalse();
        var matchResult = result.Match(
            _ => throw new Exception("Expected failure"),
            failure => failure);

        matchResult.Errors.ShouldContain("Amount must be greater than zero");
    }

    [Fact]
    public async Task DepositAsync_ShouldReturnSuccess_WhenValidRequest()
    {
        // Arrange
        _balance.Balance = 50;

        List<BalanceEntity> balances = [_balance];
        var balancesDbSetMock = balances.BuildMockDbSet();

        DbContextMock.Setup(db => db.Balances).Returns(balancesDbSetMock.Object);
        DbContextMock.Setup(db => db.SaveChangesAsync(default)).ReturnsAsync(1);

        // Act
        var result = await AccountTransactionService.DepositAsync(_userId, 100m);

        // Assert
        result.IsSuccess.ShouldBeTrue();
        _balance.Balance.ShouldBe(150m); // 50 + 100
    }

    [Fact]
    public async Task DepositAsync_ShouldReturnBadRequest_WhenSaveChangesFails()
    {
        // Arrange
        List<BalanceEntity> balances = [_balance];
        var balancesDbSetMock = balances.BuildMockDbSet();

        DbContextMock.Setup(db => db.Balances).Returns(balancesDbSetMock.Object);
        DbContextMock.Setup(db => db.SaveChangesAsync(default)).ReturnsAsync(0);

        // Act
        var result = await AccountTransactionService.DepositAsync(_userId, 100m);

        // Assert
        result.IsSuccess.ShouldBeFalse();
        var matchResult = result.Match(
            _ => throw new Exception("Expected failure"),
            failure => failure);

        matchResult.Errors.ShouldContain("Failed to complete the deposit");
    }
}
