using AutoFixture;
using BAL.Services.Account;
using DAL.Entities;
using MockQueryable.Moq;
using Moq;
using Shouldly;

namespace BAL.UnitTests.Transactions;

public class AccountTransactionServiceTransferTests : BaseAccountTransactionTests
{
    private readonly Guid _fromUserId;
    private readonly Guid _toUserId;
    private readonly BalanceEntity _fromBalance;
    private readonly BalanceEntity _toBalance;

    public AccountTransactionServiceTransferTests()
    {
        _fromUserId = Guid.NewGuid();
        _toUserId = Guid.NewGuid();

        var fromUser = Fixture.Build<UserEntity>().With(u => u.Id, _fromUserId).Create();
        var toUser = Fixture.Build<UserEntity>().With(u => u.Id, _toUserId).Create();

        _fromBalance = Fixture.Build<BalanceEntity>()
            .With(b => b.User, fromUser)
            .With(b => b.UserId, _fromUserId)
            .With(b => b.Balance, 200m)
            .Create();

        _toBalance = Fixture.Build<BalanceEntity>()
            .With(b => b.User, toUser)
            .With(b => b.UserId, _toUserId)
            .With(b => b.Balance, 50m)
            .Create();
    }

    [Fact]
    public async Task TransferAsync_ShouldReturnBadRequest_WhenUsersAreSame()
    {
        // Act
        var result = await AccountTransactionService.TransferAsync(_fromUserId, _fromUserId, 100);

        // Assert
        result.IsSuccess.ShouldBeFalse();
        var failure = result.Match(_ => throw new Exception("Expected failure"), f => f);
        failure.Errors.ShouldContain("Cannot transfer funds to the same user");
    }

    [Fact]
    public async Task TransferAsync_ShouldReturnNotFound_WhenSenderBalanceNotFound()
    {
        // Arrange
        List<BalanceEntity> balances = [_toBalance];
        DbContextMock.Setup(db => db.Balances).Returns(balances.BuildMockDbSet().Object);

        // Act
        var result = await AccountTransactionService.TransferAsync(_fromUserId, _toUserId, 100);

        // Assert
        result.IsSuccess.ShouldBeFalse();
        var failure = result.Match(_ => throw new Exception("Expected failure"), f => f);
        failure.Errors.ShouldContain("Balance for sender user not found");
    }

    [Fact]
    public async Task TransferAsync_ShouldReturnNotFound_WhenRecipientBalanceNotFound()
    {
        // Arrange
        List<BalanceEntity> balances = [_fromBalance];
        DbContextMock.Setup(db => db.Balances).Returns(balances.BuildMockDbSet().Object);

        // Act
        var result = await AccountTransactionService.TransferAsync(_fromUserId, _toUserId, 100);

        // Assert
        result.IsSuccess.ShouldBeFalse();
        var failure = result.Match(_ => throw new Exception("Expected failure"), f => f);
        failure.Errors.ShouldContain("Balance for recipient user not found");
    }

    [Fact]
    public async Task TransferAsync_ShouldReturnBadRequest_WhenNotEnoughFunds()
    {
        // Arrange
        _fromBalance.Balance = 50;
        List<BalanceEntity> balances = [_fromBalance, _toBalance];
        DbContextMock.Setup(db => db.Balances).Returns(balances.BuildMockDbSet().Object);

        // Act
        var result = await AccountTransactionService.TransferAsync(_fromUserId, _toUserId, 100);

        // Assert
        result.IsSuccess.ShouldBeFalse();
        var failure = result.Match(_ => throw new Exception("Expected failure"), f => f);
        failure.Errors.ShouldContain("Not enough funds for transfer");
    }

    [Fact]
    public async Task TransferAsync_ShouldReturnBadRequest_WhenSaveChangesFails()
    {
        // Arrange
        List<BalanceEntity> balances = [_fromBalance, _toBalance];
        DbContextMock.Setup(db => db.Balances).Returns(balances.BuildMockDbSet().Object);
        DbContextMock.Setup(db => db.SaveChangesAsync(default)).ReturnsAsync(0);

        // Act
        var result = await AccountTransactionService.TransferAsync(_fromUserId, _toUserId, 100);

        // Assert
        result.IsSuccess.ShouldBeFalse();
        var failure = result.Match(_ => throw new Exception("Expected failure"), f => f);
        failure.Errors.ShouldContain("Failed to complete the transfer");
    }

    [Fact]
    public async Task TransferAsync_ShouldReturnSuccess_WhenValidTransfer()
    {
        // Arrange
        _fromBalance.Balance = 200;
        _toBalance.Balance = 50;

        List<BalanceEntity> balances = [_fromBalance, _toBalance];
        DbContextMock.Setup(db => db.Balances).Returns(balances.BuildMockDbSet().Object);
        DbContextMock.Setup(db => db.SaveChangesAsync(default)).ReturnsAsync(1);

        // Act
        var result = await AccountTransactionService.TransferAsync(_fromUserId, _toUserId, 100);

        // Assert
        result.IsSuccess.ShouldBeTrue();
        _fromBalance.Balance.ShouldBe(100);
        _toBalance.Balance.ShouldBe(150);
    }
}
