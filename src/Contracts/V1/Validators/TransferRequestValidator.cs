namespace Contracts.V1.Validators;

using Contracts.V1.Requests.Transactions;
using FluentValidation;

public class TransferRequestValidator : AbstractValidator<TransferRequest>
{
    public TransferRequestValidator()
    {
        RuleFor(x => x.ToUserId)
            .NotEmpty().WithMessage("Recipient user ID is required");

        RuleFor(x => x.Amount)
            .GreaterThan(0).WithMessage("Amount must be greater than zero");
    }
}
