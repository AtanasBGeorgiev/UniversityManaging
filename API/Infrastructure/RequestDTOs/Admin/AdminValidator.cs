using System;
using FluentValidation;

namespace API.Infrastructure.RequestDTOs.Admin;

public class AdminValidator : AbstractValidator<AdminRequest>
{
    public AdminValidator()
    {
        RuleFor(s => s.FName)
        .NotEmpty().WithMessage("First name is required.")
        .MinimumLength(3).When(s => s.FName != null)
        .WithMessage("First name has to be at least 3 characters.");

        RuleFor(s => s.LName)
        .NotEmpty().WithMessage("Last name is required.")
        .MinimumLength(3).When(s => s.LName != null)
        .WithMessage("Last name has to be at least 3 characters.");

        RuleFor(s => s.Email)
        .NotEmpty().WithMessage("Email is required.")
        .EmailAddress().When(s => s.Email != null)
        .WithMessage("Invalid email format.");

        RuleFor(s => s.Password)
        .NotEmpty()
        .WithMessage("Password is required.")
        .Must(password =>
            password.Length >= 8 &&
            password.Any(char.IsUpper) &&
            password.Any(char.IsLower) &&
            password.Any(char.IsDigit) &&
            password.Any(ch => !char.IsLetterOrDigit(ch))
        )
        .When(s => s.Password != null)
        .WithMessage("Password has to be at least 8 characters long and contains at least one uppercase,lowercase,numeric and special symbol.");
    }
}
