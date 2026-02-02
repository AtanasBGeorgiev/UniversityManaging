using System;

namespace API.Infrastructure.RequestDTOs.AdminFaculty;

public class AdminFacultyFilterRequest
{
    public int AdminID { get; set; }
    public int FacultyID { get; set; }
}
