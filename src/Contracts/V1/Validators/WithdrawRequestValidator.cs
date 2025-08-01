namespace Contracts.V1.Validators;

using Contracts.V1.Requests.Transactions;
using FluentValidation;

public class WithdrawRequestValidator : AbstractValidator<WithdrawRequest>
{
    public WithdrawRequestValidator()
    {
        RuleFor(x => x.Amount)
            .GreaterThan(0).WithMessage("Amount must be greater than zero");
    }
}
