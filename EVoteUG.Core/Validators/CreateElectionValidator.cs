using EVoteUG.Core.DTOs.Election;
using FluentValidation;

namespace EVoteUG.Core.Validators;

public class CreateElectionValidator : AbstractValidator<CreateElectionRequestDto>
{
    public CreateElectionValidator()
    {
        RuleFor(x => x.Title)
            .NotEmpty().WithMessage("Election title is required.")
            .MaximumLength(200).WithMessage("Election title cannot exceed 200 characters.");

        RuleFor(x => x.AcademicYear)
            .NotEmpty().WithMessage("Academic year is required.")
            .Matches(@"^\d{4}/\d{4}$").WithMessage("Academic year must be in format YYYY/YYYY (e.g. 2026/2027).");

        RuleFor(x => x.StartDate)
            .NotEmpty().WithMessage("Start date is required.");

        RuleFor(x => x.EndDate)
            .NotEmpty().WithMessage("End date is required.")
            .GreaterThan(x => x.StartDate).WithMessage("End date must be after the start date.");
    }
}
