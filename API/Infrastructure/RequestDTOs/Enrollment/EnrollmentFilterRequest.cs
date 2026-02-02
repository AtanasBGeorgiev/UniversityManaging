using System;

namespace API.Infrastructure.RequestDTOs.Enrollment;

public class EnrollmentFilterRequest
{
    public int StudentID { get; set; }
    public int CourseID { get; set; }
    public string Year { get; set; }
    public int Semester { get; set; }
    public int Grade { get; set; }
}
