using DAL.Entities;
using Microsoft.AspNetCore.Identity;
using Moq;

namespace Base.UnitTests.Extensions;

public static class UserManagerMockExtensions
{
    public static void SetupFindByEmailAsync(this Mock<UserManager<UserEntity>> userManagerMock, UserEntity? userEntity)
    {
        userManagerMock
            .Setup(userManager => userManager.FindByEmailAsync(It.IsAny<string>()))
            .ReturnsAsync(userEntity);
    }

    public static void VerifyFindByEmailAsync(this Mock<UserManager<UserEntity>> userManagerMock,
        string email,
        Func<Times>? times = null)
    {
        userManagerMock
            .Verify(userManger => userManger.FindByEmailAsync(email), times ?? Times.Once);
    }

    public static void SetupCheckPasswordAsync(this Mock<UserManager<UserEntity>> userManagerMock, bool isValid)
    {
        userManagerMock
            .Setup(userManager => userManager.CheckPasswordAsync(It.IsAny<UserEntity>(), It.IsAny<string>()))
            .ReturnsAsync(isValid);
    }

    public static void VerifyCheckPasswordAsync(this Mock<UserManager<UserEntity>> userManagerMock,
        UserEntity userEntity,
        string password,
        Func<Times>? times = null)
    {
        userManagerMock
            .Verify(userManager => userManager.CheckPasswordAsync(userEntity, password), times ?? Times.Once);
    }

    public static void SetupIsEmailConfirmedAsync(this Mock<UserManager<UserEntity>> userManagerMock, bool isConfirmed)
    {
        userManagerMock
            .Setup(userManager => userManager.IsEmailConfirmedAsync(It.IsAny<UserEntity>()))
            .ReturnsAsync(isConfirmed);
    }

    public static void VerifyIsEmailConfirmedAsync(this Mock<UserManager<UserEntity>> userManagerMock,
        UserEntity userEntity,
        Func<Times>? times = null)
    {
        userManagerMock
            .Verify(userManager => userManager.IsEmailConfirmedAsync(userEntity), times ?? Times.Once);
    }

    public static void SetupIsLockedOutAsync(this Mock<UserManager<UserEntity>> userManagerMock, bool isLockedOut)
    {
        userManagerMock
            .Setup(userManager => userManager.IsLockedOutAsync(It.IsAny<UserEntity>()))
            .ReturnsAsync(isLockedOut);
    }

    public static void VerifyIsLockedOutAsync(this Mock<UserManager<UserEntity>> userManagerMock,
        UserEntity userEntity,
        Func<Times>? times = null)
    {
        userManagerMock
            .Verify(userManager => userManager.IsLockedOutAsync(userEntity), times ?? Times.Once);
    }

    public static void VerifyCreateAsync(this Mock<UserManager<UserEntity>> userManagerMock,
        string email,
        string password,
        Func<Times>? times = null)
    {
        userManagerMock.Verify(userManager =>
            userManager.CreateAsync(
                It.Is<UserEntity>(userEntity => userEntity.Email == email), password
            ), times ?? Times.Once
        );
    }

    public static void VerifyCreateAsync(this Mock<UserManager<UserEntity>> userManagerMock,
        string email,
        Func<Times>? times = null)
    {
        userManagerMock.Verify(userManager =>
            userManager.CreateAsync(
                It.Is<UserEntity>(userEntity => userEntity.Email == email)), times ?? Times.Once
        );
    }
}
