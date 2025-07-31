using BAL.Services.Account;
using Contracts;
using Contracts.V1.Requests.Account;
using Contracts.V1.Responses;
using Contracts.V1.Responses.Account;
using Mappers.Mapping;
using Microsoft.AspNetCore.Mvc;
using WebApi.Extensions;

namespace WebApi.Controllers.V1;

/// <summary>
/// Provides API endpoints for managing user authentication, identification and authorization.
/// </summary>
public class AccountController(IAccontManagement accontManagement) : ControllerBase
{
    /// <summary>
    /// Logs in user and returns user information, otherwise error is returned.
    /// </summary>
    /// <remarks>
    /// If incorrect credentials are provided,
    /// a <see cref="FailureResponse"/> with an error message is returned.
    /// </remarks>
    /// <response code="200">Returns user information.</response>
    /// <response code="401">Returns unauthorized error.</response>
    [HttpPost(ApiRoutes.Account.Login)]
    [ProducesResponseType(typeof(AuthResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(FailureResponse), StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> Login([FromBody] LoginRequest loginRequest)
    {
        var result = await accontManagement.LoginAsync(loginRequest.Email, loginRequest.Password);

        return result.Match(
            authSuccessDto =>
            {
                return Ok(authSuccessDto.ToResponse());
            },
            failure => failure.ToActionResult()
        );
    }

    /// <summary>
    /// Signs up user and returns user information, otherwise error is returned.
    /// </summary>
    /// <remarks>
    /// If a user with the same username already exists or an error occurs,
    /// a <see cref="FailureResponse"/> with an error message is returned.
    /// </remarks>
    /// <response code="200">Returns user information.</response>
    /// <response code="400">Returns bad request error.</response>
    /// <response code="409">Returns conflict error.</response>
    [HttpPost(ApiRoutes.Account.Signup)]
    [ProducesResponseType(typeof(AuthResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(FailureResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(FailureResponse), StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Signup([FromBody] SignupRequest signupRequest)
    {
        var result = await accontManagement.SignupAsync(signupRequest.Email, signupRequest.Password);

        return result.Match(
            authSuccessDto =>
            {
                return Ok(authSuccessDto.ToResponse());
            },
            failure => failure.ToActionResult()
        );
    }
}
