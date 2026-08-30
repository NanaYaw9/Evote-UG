using EVoteUG.Core.DTOs.Auth;
using FluentValidation;

namespace EVoteUG.Core.Validators;

public class StudentLoginValidator : AbstractValidator<StudentLoginRequestDto>
{
    public StudentLoginValidator()
    {
        RuleFor(x => x.StudentId)
            .NotEmpty().WithMessage("Student ID is required.")
            .Matches(@"^\d{7,10}$").WithMessage("Student ID must be a valid 7 to 10 digit university number.");

        RuleFor(x => x.Password)
            .NotEmpty().WithMessage("Password is required.")
            .MinimumLength(6).WithMessage("Password must be at least 6 characters.");
    }
}
