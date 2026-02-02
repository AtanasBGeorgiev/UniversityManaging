using System;

namespace API.Infrastructure.RequestDTOs.Enrollment;

public class EnrollmentRequest
{
    public int StudentID { get; set; }
    public int CourseID { get; set; }
}
