using System;
using FluentValidation;

namespace API.Infrastructure.RequestDTOs.Faculty;

public class FacultyValidator : AbstractValidator<FacultyRequest>
{
    public FacultyValidator()
    {
        RuleFor(f => f.Name)
        .NotEmpty().When(f => f.Name != null)
        .WithMessage("Name of the faculty is required.");
    }
}
