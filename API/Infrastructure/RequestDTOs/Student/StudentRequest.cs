using System;
using API.Infrastructure.RequestDTOs.Shared;

namespace API.Infrastructure.RequestDTOs;

public class StudentRequest : PersonRequest
{
    public int FacultyId { get; set; }
}