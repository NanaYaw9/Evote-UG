using EVoteUG.Core.DTOs.Auth;
using FluentValidation;

namespace EVoteUG.Core.Validators;

public class StudentRegisterValidator : AbstractValidator<StudentRegisterRequestDto>
{
    public StudentRegisterValidator()
    {
        RuleFor(x => x.StudentId)
            .NotEmpty().WithMessage("Student ID is required.")
            .Matches(@"^\d{7,10}$").WithMessage("Student ID must be a valid 7 to 10 digit university number.");

        RuleFor(x => x.FullName)
            .NotEmpty().WithMessage("Full name is required.")
            .MaximumLength(150).WithMessage("Full name must not exceed 150 characters.");

        RuleFor(x => x.Email)
            .NotEmpty().WithMessage("Email address is required.")
            .EmailAddress().WithMessage("A valid email address is required.")
            .MaximumLength(150).WithMessage("Email must not exceed 150 characters.");

        RuleFor(x => x.Password)
            .NotEmpty().WithMessage("Password is required.")
            .MinimumLength(6).WithMessage("Password must be at least 6 characters.");
    }
}
