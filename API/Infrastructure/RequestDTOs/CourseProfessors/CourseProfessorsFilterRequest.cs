using System;

namespace API.Infrastructure.RequestDTOs.CourseProfessors;

public class CourseProfessorsFilterRequest
{
    public int CourseID { get; set; }
    public int ProfessorID { get; set; }
}
