using CardActions.Application.Features.CardActions.Queries.GetAllowedCardActions;
using FluentValidation;

namespace CardActions.Api.Validators;

public class GetAllowedCardActionsRequestValidator : AbstractValidator<GetAllowedCardActionsQuery>
{
    public GetAllowedCardActionsRequestValidator()
    {
        RuleFor(x => x.UserId)
            .NotEmpty()
            .WithMessage("Validation.UserId.Required")
            .MaximumLength(50)
            .WithMessage("Validation.UserId.TooLong")
            .Matches("^[A-Za-z0-9]+$")
            .WithMessage("Validation.UserId.InvalidFormat");

        RuleFor(x => x.CardNumber)
            .NotEmpty()
            .WithMessage("Validation.CardNumber.Required")
            .MaximumLength(20)
            .WithMessage("Validation.CardNumber.TooLong")
            .Matches("^[A-Za-z0-9]+$")
            .WithMessage("Validation.CardNumber.InvalidFormat");
    }
} 