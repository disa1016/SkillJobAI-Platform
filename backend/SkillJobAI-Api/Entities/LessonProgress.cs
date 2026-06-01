namespace SkillJobAI.Api.Entities;

public class LessonProgress
{
    public int Id { get; set; }

    public int UserId { get; set; }

    public int LessonId { get; set; }

    public DateTime CompletedAt { get; set; } = DateTime.UtcNow;

    public AppUser? User { get; set; }

    public Lesson? Lesson { get; set; }
}