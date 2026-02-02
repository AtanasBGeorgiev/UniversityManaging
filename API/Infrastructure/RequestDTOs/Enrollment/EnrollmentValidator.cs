using System;
using FluentValidation;

namespace API.Infrastructure.RequestDTOs.Enrollment;

public class EnrollmentValidator : AbstractValidator<EnrollmentFilterRequest>
{
    public EnrollmentValidator()
    {
        RuleFor(e => e.Year)
        .NotNull()
        .Matches(@"^\d{4}/\d{4}$")
        .WithMessage("Value must be in format 'YYYY/YYYY'.")
        .Must(value =>
        {
            var parts = value.Split('/');
            return int.TryParse(parts[0], out int start) &&
            int.TryParse(parts[1], out int end) &&
            end == start + 1;
        })
        .WithMessage("Second year have to be exactly one year greater than the first.");

        RuleFor(e => e.Semester)
        .NotNull()
        .WithMessage("Semester is required.")
        .InclusiveBetween(1, 2)
        .WithMessage("Semester have to be 1 or 2.");

        RuleFor(e => e.Grade)
        .InclusiveBetween(2, 6)
        .When(e => e.Grade != 0)
        .WithMessage("Grade have to be from 2 to 6.");
    }
}
