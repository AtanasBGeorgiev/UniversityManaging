using System;
using API.Infrastructure.RequestDTOs.Shared;

namespace API.Infrastructure.RequestDTOs.Student;

public class StudentFilterRequest : PersonFilterRequest
{
    public int FacultyId { get; set; }
}
