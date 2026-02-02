using System;

namespace API.Infrastructure.RequestDTOs.Course;

public class CourseRequest
{
    public string Title { get; set; }
    public int Credits { get; set; }
    public int FacultyID { get; set; }
}
