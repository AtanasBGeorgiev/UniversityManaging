using System;
using FluentValidation;

namespace API.Infrastructure.RequestDTOs.CourseProfessors;

public class CourseProfessorValidator : AbstractValidator<CourseProfessorsRequest>
{
    public CourseProfessorValidator()
    {
        RuleFor(cp => cp.CourseID)
        .NotNull()
        .WithMessage("CourseID is required.");

        RuleFor(cp => cp.ProfessorID)
        .NotNull()
        .WithMessage("ProfessorID is required.");
    }
}
