using EVoteUG.Core.DTOs.Admin;
using FluentValidation;

namespace EVoteUG.Core.Validators;

public class AdminRegisterValidator : AbstractValidator<AdminRegisterRequestDto>
{
    public AdminRegisterValidator()
    {
        RuleFor(x => x.Username)
            .NotEmpty().WithMessage("Username is required.")
            .MinimumLength(3).WithMessage("Username must be at least 3 characters.")
            .MaximumLength(50).WithMessage("Username must not exceed 50 characters.");

        RuleFor(x => x.Password)
            .NotEmpty().WithMessage("Password is required.")
            .MinimumLength(6).WithMessage("Password must be at least 6 characters.");
    }
}
