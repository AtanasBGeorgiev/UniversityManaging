using System;
using API.Infrastructure.RequestDTOs.Shared;

namespace API.Infrastructure.RequestDTOs.Professor;

public class ProfessorFilterRequest : PersonFilterRequest
{
    public int FacultyId { get; set; }
}
