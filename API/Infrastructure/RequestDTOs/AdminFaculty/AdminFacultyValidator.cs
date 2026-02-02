using System;
using FluentValidation;

namespace API.Infrastructure.RequestDTOs.AdminFaculty;

public class AdminFacultyValidator : AbstractValidator<AdminFacultyRequest>
{
    public AdminFacultyValidator()
    {
        RuleFor(af => af.AdminID)
        .NotNull()
        .WithMessage("AdminID is required.");

        RuleFor(cp => cp.FacultyID)
        .NotNull()
        .WithMessage("FacultyID is required.");
    }
}
