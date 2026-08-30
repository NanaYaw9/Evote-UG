using EVoteUG.Core.DTOs.Position;
using FluentValidation;

namespace EVoteUG.Core.Validators;

public class CreatePositionValidator : AbstractValidator<CreatePositionRequestDto>
{
    public CreatePositionValidator()
    {
        RuleFor(x => x.ElectionId)
            .GreaterThan(0).WithMessage("Valid Election ID is required.");

        RuleFor(x => x.Title)
            .NotEmpty().WithMessage("Position title is required.")
            .MaximumLength(100).WithMessage("Position title cannot exceed 100 characters.");

        RuleFor(x => x.MaxVotesAllowed)
            .GreaterThanOrEqualTo(1).WithMessage("Max votes allowed must be at least 1.");
    }
}
