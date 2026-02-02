using System;

namespace Common.Entities;

public class AdminFaculty : BaseEntity
{
    public int AdminID { get; set; }
    public int FacultyID { get; set; }

    public virtual Admin Admin { get; set; }
    public virtual Faculty Faculty { get; set; }

    public override int[] GetIds()
    {
        return [AdminID, FacultyID];
    }
}