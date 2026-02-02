using System;

namespace Common.Entities;

public class StudentProfessor : BaseEntity
{
    public int StudentID { get; set; }
    public int ProfessorID { get; set; }
    public virtual Student Student { get; set; }
    public virtual Professor Professor { get; set; }

    public override int[] GetIds()
    {
        return [StudentID, ProfessorID];
    }
}