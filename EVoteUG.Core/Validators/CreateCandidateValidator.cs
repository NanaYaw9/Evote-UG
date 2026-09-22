using EVoteUG.Core.DTOs.Candidate;
using FluentValidation;

namespace EVoteUG.Core.Validators;

public class CreateCandidateValidator : AbstractValidator<CreateCandidateRequestDto>
{
    public CreateCandidateValidator()
    {
        RuleFor(x => x.PositionId)
            .GreaterThan(0).WithMessage("Valid Position ID is required.");

        RuleFor(x => x.FullName)
            .NotEmpty().WithMessage("Candidate full name is required.")
            .MaximumLength(150).WithMessage("Candidate full name cannot exceed 150 characters.");

        RuleFor(x => x.StudentId)
            .NotEmpty().WithMessage("Candidate student ID is required.");
    }
}
