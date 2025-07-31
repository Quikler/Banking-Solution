using System.ComponentModel.DataAnnotations;

namespace Contracts.V1.Requests.Account;

public record SignupRequest
{
    public required string Email { get; set; }

    [Compare(nameof(ConfirmPassword), ErrorMessage = "Passwords do not match.")]
    public required string Password { get; set; }
    public required string ConfirmPassword { get; set; }
}
