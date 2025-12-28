using System;
using API.Infrastructure.RequestDTOs.Shared;

namespace API.Infrastructure.RequestDTOs.Professor;

public class ProfessorRequest : PersonRequest
{
    public int FacultyId { get; set; }
}
