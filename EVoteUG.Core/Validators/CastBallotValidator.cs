using EVoteUG.Core.DTOs.Voting;
using FluentValidation;

namespace EVoteUG.Core.Validators;

public class CastBallotValidator : AbstractValidator<CastBallotRequestDto>
{
    public CastBallotValidator()
    {
        RuleFor(x => x.ElectionId)
            .GreaterThan(0).WithMessage("Valid Election ID is required.");

        RuleFor(x => x.Selections)
            .NotEmpty().WithMessage("Ballot must contain at least one vote selection.");

        RuleForEach(x => x.Selections).ChildRules(selection =>
        {
            selection.RuleFor(s => s.PositionId)
                .GreaterThan(0).WithMessage("Valid Position ID is required.");

            selection.RuleFor(s => s.CandidateId)
                .GreaterThan(0).WithMessage("Valid Candidate ID is required.");
        });

        // Ensure no duplicate position voting in the same ballot payload
        RuleFor(x => x.Selections)
            .Must(selections => selections.Select(s => s.PositionId).Distinct().Count() == selections.Count)
            .WithMessage("A ballot cannot contain multiple votes for the same position.");
    }
}
