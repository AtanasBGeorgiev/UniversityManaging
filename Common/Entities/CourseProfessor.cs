using System;

namespace Common.Entities;

public class CourseProfessor
{
    public int CourseID { get; set; }
    public int ProfessorID { get; set; }

    public virtual Course Course { get; set; }
    public virtual Professor Professor { get; set; }
}
