using System;
using FluentValidation;

namespace API.Infrastructure.RequestDTOs.StudentProfessors;

public class StudentProfessorsValidator : AbstractValidator<StudentProfessorsRequest>
{
    public StudentProfessorsValidator()
    {
        RuleFor(cp => cp.StudentID)
        .NotNull()
        .WithMessage("StudentID is required.");

        RuleFor(cp => cp.ProfessorID)
        .NotNull()
        .WithMessage("ProfessorID is required.");
    }
}
