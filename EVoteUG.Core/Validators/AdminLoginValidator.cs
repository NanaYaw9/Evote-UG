using EVoteUG.Core.DTOs.Auth;
using FluentValidation;

namespace EVoteUG.Core.Validators;

public class AdminLoginValidator : AbstractValidator<AdminLoginRequestDto>
{
    public AdminLoginValidator()
    {
        RuleFor(x => x.Username)
            .NotEmpty().WithMessage("Username is required.")
            .MinimumLength(3).WithMessage("Username must be at least 3 characters.");

        RuleFor(x => x.Password)
            .NotEmpty().WithMessage("Password is required.");
    }
}
