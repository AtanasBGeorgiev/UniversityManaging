using System;
using Common.Entities;
using DotNetEnv;
using Microsoft.EntityFrameworkCore;

namespace Common.Persistance;

public class AppDbContext : DbContext
{
    public DbSet<Role> Roles { get; set; }
    public DbSet<Admin> Admins { get; set; }
    public DbSet<Faculty> Faculties { get; set; }
    public DbSet<Student> Students { get; set; }
    public DbSet<Professor> Professors { get; set; }
    public DbSet<Course> Courses { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        optionsBuilder
            .UseLazyLoadingProxies()
            .UseSqlServer(@"YOUR_DB_CONNECTION_STRING");
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        #region Role

        modelBuilder.Entity<Role>()
        .HasKey(r => r.RoleID);

        modelBuilder.Entity<Role>()
        .Property(r => r.RoleID)
        .ValueGeneratedOnAdd();

        modelBuilder.Entity<Role>()
        .HasIndex(r => r.RoleName)
        .IsUnique();

        modelBuilder.Entity<Role>()
        .HasData(
            new Role { RoleID = 1, RoleName = "Admin" },
            new Role { RoleID = 2, RoleName = "Professor" },
            new Role { RoleID = 3, RoleName = "Student" }
        );

        modelBuilder.Entity<Role>()
        .ToTable(t => t.HasCheckConstraint("CK_Role_RoleName_Constraint",
        "RoleName IN ('Admin','Professor','Student')"));

        #endregion

        #region Faculty

        modelBuilder.Entity<Faculty>()
        .HasKey(f => f.FacultyID);

        modelBuilder.Entity<Faculty>()
        .Property(f => f.FacultyID)
        .ValueGeneratedOnAdd();

        modelBuilder.Entity<Faculty>()
        .HasIndex(f => f.DeanID)
        .IsUnique();

        modelBuilder.Entity<Faculty>()
        .Property(f => f.DeanID)
        .IsRequired(false);

        modelBuilder.Entity<Faculty>()
        .HasMany(f => f.Admins)
        .WithMany()
        .UsingEntity<AdminFaculty>(
            af => af
            .HasOne(af => af.Admin)
            .WithMany()
            .HasForeignKey(af => af.AdminID),
            af => af
            .HasOne(af => af.Faculty)
            .WithMany()
            .HasForeignKey(af => af.FacultyID),
            af => af
            .HasKey(t => new { t.AdminID, t.FacultyID })
        );

        #endregion

        #region Student

        modelBuilder.Entity<Student>()
        .HasKey(s => s.StudentID);

        modelBuilder.Entity<Student>()
        .Property(s => s.StudentID)
        .ValueGeneratedOnAdd();

        modelBuilder.Entity<Student>()
        .HasOne(s => s.Faculty)
        .WithMany()
        .HasForeignKey(s => s.FacultyID)
        .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<Student>()
        .HasOne(s => s.Role)
        .WithMany()
        .HasForeignKey(s => s.RoleID)
        .OnDelete(DeleteBehavior.Restrict);

        #endregion

        #region Professor

        modelBuilder.Entity<Professor>()
        .HasKey(p => p.ProfessorID);

        modelBuilder.Entity<Professor>()
        .Property(p => p.ProfessorID)
        .ValueGeneratedOnAdd();

        modelBuilder.Entity<Professor>()
        .HasOne(p => p.Role)
        .WithMany()
        .HasForeignKey(p => p.RoleID);

        modelBuilder.Entity<Professor>()
        .HasOne(p => p.Faculty)
        .WithMany()
        .HasForeignKey(p => p.FacultyID)
        .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<Professor>()
        .HasOne(p => p.DeanOfFaculty)
        .WithOne()
        .HasForeignKey<Faculty>(f => f.DeanID)
        .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<Professor>(
            p => p
            .HasMany(p => p.Students)
            .WithMany()
            .UsingEntity<StudentProfessor>(
                sp => sp
                .HasOne(sp => sp.Student)
                .WithMany()
                .HasForeignKey(sp => sp.StudentID),
                sp => sp
                .HasOne(sp => sp.Professor)
                .WithMany()
                .HasForeignKey(sp => sp.ProfessorID),
                sp => sp
                .HasKey(t => new { t.StudentID, t.ProfessorID }))
        );

        #endregion

        #region Course

        modelBuilder.Entity<Course>()
        .HasKey(c => c.CourseID);

        modelBuilder.Entity<Course>()
        .Property(c => c.CourseID)
        .ValueGeneratedOnAdd();

        modelBuilder.Entity<Course>()
        .HasOne(c => c.Faculty)
        .WithMany()
        .HasForeignKey(c => c.FacultyID)
        .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<Course>()
        .HasMany(c => c.Professors)
        .WithMany()
        .UsingEntity<CourseProfessor>(
            cp => cp
            .HasOne(cp => cp.Professor)
            .WithMany()
            .HasForeignKey(cp => cp.ProfessorID),
             cp => cp
            .HasOne(cp => cp.Course)
            .WithMany()
            .HasForeignKey(cp => cp.CourseID),
            cp => cp
            .HasKey(t => new { t.CourseID, t.ProfessorID })
        );

        modelBuilder.Entity<Course>()
        .HasMany(c => c.Students)
        .WithMany()
        .UsingEntity<Enrollment>(
            e => e
            .HasOne(e => e.Student)
            .WithMany()
            .HasForeignKey(e => e.StudentID),
            e => e
            .HasOne(e => e.Course)
            .WithMany()
            .HasForeignKey(e => e.CourseID),
            e => e
            .HasKey(k => new { k.StudentID, k.CourseID })
        );

        #endregion

        #region Enrollment

        modelBuilder.Entity<Enrollment>()
        .Property(e => e.Grade)
        .IsRequired(false);

        modelBuilder.Entity<Enrollment>()
        .ToTable(t => t.HasCheckConstraint("CK_Enrollment_Grade_Constraint",
        "Grade IN (2,3,4,5,6) OR Grade IS NULL"));

        modelBuilder.Entity<Enrollment>()
        .ToTable(t => t.HasCheckConstraint("CK_Enrollment_Semester_Constraint",
        "Semester IN (1,2)"));

        #endregion

        #region Admin

        modelBuilder.Entity<Admin>().ToTable("Admins");

        modelBuilder.Entity<Admin>()
        .HasKey(a => a.AdminID);

        modelBuilder.Entity<Admin>()
        .Property(a => a.AdminID)
        .ValueGeneratedOnAdd();

        modelBuilder.Entity<Admin>()
        .HasOne(a => a.Role)
        .WithMany()
        .HasForeignKey(a => a.RoleID)
        .OnDelete(DeleteBehavior.Restrict);

        #endregion
    }
}