using System;

namespace API.Infrastructure.RequestDTOs.StudentProfessors;

public class StudentProfessorsFilterRequest
{
    public int StudentID { get; set; }
    public int ProfessorID { get; set; }
}
