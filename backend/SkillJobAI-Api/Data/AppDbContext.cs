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
    public DbSet<Company> Companies => Set<Company>();
    public DbSet<CompanyMember> CompanyMembers => Set<CompanyMember>();
    public DbSet<Skill> Skills => Set<Skill>();
    public DbSet<JobSkill> JobSkills => Set<JobSkill>();
    public DbSet<CourseSkill> CourseSkills => Set<CourseSkill>();
    public DbSet<UserSkill> UserSkills => Set<UserSkill>();
    public DbSet<CareerGoal> CareerGoals => Set<CareerGoal>();
    public DbSet<CareerGoalSkill> CareerGoalSkills => Set<CareerGoalSkill>();
    public DbSet<UserCareerGoal> UserCareerGoals => Set<UserCareerGoal>();

    public DbSet<PasswordResetToken> PasswordResetTokens =>
        Set<PasswordResetToken>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<Job>()
            .HasOne(j => j.Company)
            .WithMany(c => c.Jobs)
            .HasForeignKey(j => j.CompanyId)
            .OnDelete(DeleteBehavior.SetNull);

        modelBuilder.Entity<CompanyMember>()
            .HasOne(cm => cm.User)
            .WithMany()
            .HasForeignKey(cm => cm.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<CompanyMember>()
            .HasOne(cm => cm.Company)
            .WithMany(c => c.Members)
            .HasForeignKey(cm => cm.CompanyId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<JobSkill>()
            .HasOne(js => js.Job)
            .WithMany()
            .HasForeignKey(js => js.JobId);

        modelBuilder.Entity<JobSkill>()
            .HasOne(js => js.Skill)
            .WithMany()
            .HasForeignKey(js => js.SkillId);

        modelBuilder.Entity<CourseSkill>()
            .HasOne(cs => cs.Course)
            .WithMany()
            .HasForeignKey(cs => cs.CourseId);

        modelBuilder.Entity<CourseSkill>()
            .HasOne(cs => cs.Skill)
            .WithMany()
            .HasForeignKey(cs => cs.SkillId);

        modelBuilder.Entity<UserSkill>()
            .HasOne(us => us.User)
            .WithMany()
            .HasForeignKey(us => us.UserId);

        modelBuilder.Entity<UserSkill>()
            .HasOne(us => us.Skill)
            .WithMany()
            .HasForeignKey(us => us.SkillId);

        modelBuilder.Entity<PasswordResetToken>(entity =>
        {
            entity.HasKey(token => token.Id);

            entity.Property(token => token.TokenHash)
                .IsRequired()
                .HasMaxLength(64);

            entity.HasIndex(token => token.TokenHash)
                .IsUnique();

            entity.HasIndex(token => new
            {
                token.UserId,
                token.ExpiresAt
            });

            entity.HasOne(token => token.User)
                .WithMany()
                .HasForeignKey(token => token.UserId)
                .OnDelete(DeleteBehavior.Cascade);
        });
    }
}