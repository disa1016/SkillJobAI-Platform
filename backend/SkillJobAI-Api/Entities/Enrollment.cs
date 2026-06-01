namespace SkillJobAI.Api.Entities;

public class Enrollment
{
    public int Id { get; set; }

    public int UserId { get; set; }

    public int CourseId { get; set; }

    public DateTime EnrolledAt { get; set; } = DateTime.UtcNow;

    public bool IsCompleted { get; set; } = false;

    public AppUser? User { get; set; }

    public Course? Course { get; set; }
}