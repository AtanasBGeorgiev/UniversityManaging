using System;
using FluentValidation;

namespace API.Infrastructure.RequestDTOs.Course;

public class CourseValidator : AbstractValidator<CourseRequest>
{
    public CourseValidator()
    {
        RuleFor(c => c.Title)
        .NotEmpty().WithMessage("First name is required.")
        .MinimumLength(3).When(c => c.Title != null)
        .WithMessage("Title has to be at least 3 characters.");

        RuleFor(c => c.Credits)
        .NotEmpty().WithMessage("Credits is required.")
        .InclusiveBetween(1, 10).When(c => c.Title != null)
        .WithMessage("Cretits have to be from 1 to 10.");
    }
}
