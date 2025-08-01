using Contracts.V1.Requests.Account;
using Contracts.V1.Responses;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;
using Common.DTOs;
using Contracts.V1.Responses.Account;
using Shouldly;

namespace WebApi.UnitTests.Accounts;

public class AccountsControllerTests : BaseAccountsController
{
    private readonly LoginUserRequest _loginUserRequest;
    private readonly BalanceDto _balanceDto;

    public AccountsControllerTests()
    {
        _loginUserRequest = new LoginUserRequest
        {
            Email = "test@example.com",
            Password = "password123"
        };

        _balanceDto = new BalanceDto
        {
            Balance = 0,
            Id = Guid.NewGuid(),
            UserId = Guid.NewGuid(),
        };
    }

    [Fact]
    public async Task Login_ShouldReturnOk_WhenLoginSuccessful()
    {
        // Arrange
        var authSuccessDto = new AuthSuccessDto
        {
            Token = "jwt-token",
            RefreshToken = "refresh-token",
            User = new UserDto
            {
                Balance = _balanceDto,
                Id = Guid.NewGuid(),
                Email = "test@example.com",
            }
        };

        AccountManagementService
            .Setup(x => x.LoginAsync(It.IsAny<LoginUserDto>()))
            .ReturnsAsync(authSuccessDto);

        // Act
        var result = await Controller.Login(_loginUserRequest);

        // Assert
        var okResult = result.ShouldBeOfType<OkObjectResult>();
        var response = okResult.Value.ShouldBeOfType<AuthResponse>();
        response.Token.ShouldBe(authSuccessDto.Token);
        response.User.Email.ShouldBe(authSuccessDto.User.Email);

        Controller.HttpContext.Response.Headers.SetCookie
            .ToString()
            .ShouldContain("refreshToken=refresh-token");
    }

    [Fact]
    public async Task Login_ShouldReturnUnauthorized_WhenLoginFails()
    {
        // Arrange
        var loginRequest = new LoginUserRequest
        {
            Email = "fail@example.com",
            Password = "wrongpassword"
        };

        var failureDto = FailureDto.Unauthorized("Invalid email or password");

        AccountManagementService
            .Setup(x => x.LoginAsync(It.IsAny<LoginUserDto>()))
            .ReturnsAsync(failureDto);

        // Act
        var result = await Controller.Login(loginRequest);

        // Assert
        var unauthorizedResult = result.ShouldBeOfType<UnauthorizedObjectResult>();
        unauthorizedResult.StatusCode.ShouldBe(StatusCodes.Status401Unauthorized);

        var failureResponse = unauthorizedResult.Value.ShouldBeOfType<FailureResponse>();
        failureResponse.Errors.ShouldContain("Invalid email or password");
    }
}
