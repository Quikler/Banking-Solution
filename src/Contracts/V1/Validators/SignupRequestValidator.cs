namespace Contracts.V1.Validators;

using Contracts.V1.Requests.Account;
using FluentValidation;

public class SignupRequestValidator : AbstractValidator<SignupUserRequest>
{
    public SignupRequestValidator()
    {
        RuleFor(x => x.Email)
            .NotEmpty().WithMessage("Email is required")
            .EmailAddress().WithMessage("Email format is invalid");

        RuleFor(x => x.Password)
            .NotEmpty().WithMessage("Password is required");

        RuleFor(x => x.ConfirmPassword)
            .Equal(x => x.Password)
            .WithMessage("Passwords do not match");
    }
}
