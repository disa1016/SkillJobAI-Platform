using Microsoft.EntityFrameworkCore;
using SkillJobAI.Api.Entities;

namespace SkillJobAI.Api.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options)
        : base(options)
    {
    }

    public DbSet<AppUser> Users => Set<AppUser>();

    public DbSet<Job> Jobs => Set<Job>();

    public DbSet<Application> Applications => Set<Application>();

    public DbSet<Course> Courses => Set<Course>();
    public DbSet<Lesson> Lessons => Set<Lesson>();
    public DbSet<Enrollment> Enrollments => Set<Enrollment>();

    public DbSet<LessonProgress> LessonProgresses => Set<LessonProgress>();


}