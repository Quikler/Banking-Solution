using Contracts.V1.Requests.Account;
using Contracts.V1.Responses;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;
using Common.DTOs;
using Contracts.V1.Responses.Account;
using Shouldly;

namespace WebApi.UnitTests.Accounts;

public class AccountsControllerSignupTests : BaseAccountsController
{
    private readonly SignupUserRequest _signupUserRequest;
    private readonly BalanceDto _balanceDto;

    public AccountsControllerSignupTests()
    {
        _signupUserRequest = new SignupUserRequest
        {
            Email = "newuser@example.com",
            Password = "password123",
            ConfirmPassword = "password123"
        };

        _balanceDto = new BalanceDto
        {
            Balance = 0,
            Id = Guid.NewGuid(),
            UserId = Guid.NewGuid(),
        };
    }

    [Fact]
    public async Task Signup_ShouldReturnOk_WhenSignupSuccessful()
    {
        // Arrange
        var authSuccessDto = new AuthSuccessDto
        {
            Token = "jwt-token",
            RefreshToken = "refresh-token",
            User = new UserDto
            {
                Id = Guid.NewGuid(),
                Email = _signupUserRequest.Email,
                Balance = _balanceDto,
            }
        };

        AccountManagementService
            .Setup(x => x.SignupAsync(It.IsAny<SignupUserDto>()))
            .ReturnsAsync(authSuccessDto);

        // Act
        var result = await Controller.Signup(_signupUserRequest);

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
    public async Task Signup_ShouldReturnConflict_WhenEmailAlreadyExists()
    {
        // Arrange
        var failureDto = FailureDto.Conflict("Email already exist");

        AccountManagementService
            .Setup(x => x.SignupAsync(It.IsAny<SignupUserDto>()))
            .ReturnsAsync(failureDto);

        // Act
        var result = await Controller.Signup(_signupUserRequest);

        // Assert
        var conflictResult = result.ShouldBeOfType<ConflictObjectResult>();
        conflictResult.StatusCode.ShouldBe(StatusCodes.Status409Conflict);

        var failureResponse = conflictResult.Value.ShouldBeOfType<FailureResponse>();
        failureResponse.Errors.ShouldContain("Email already exist");
    }

    [Fact]
    public async Task Signup_ShouldReturnBadRequest_WhenOtherValidationErrors()
    {
        // Arrange
        var failureDto = FailureDto.BadRequest(["Password too weak", "Email invalid"]);

        AccountManagementService
            .Setup(x => x.SignupAsync(It.IsAny<SignupUserDto>()))
            .ReturnsAsync(failureDto);

        // Act
        var result = await Controller.Signup(_signupUserRequest);

        // Assert
        var badRequestResult = result.ShouldBeOfType<BadRequestObjectResult>();
        badRequestResult.StatusCode.ShouldBe(StatusCodes.Status400BadRequest);

        var failureResponse = badRequestResult.Value.ShouldBeOfType<FailureResponse>();
        failureResponse.Errors.ShouldContain("Password too weak");
        failureResponse.Errors.ShouldContain("Email invalid");
    }
}
