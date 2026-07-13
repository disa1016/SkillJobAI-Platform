namespace SkillJobAI.Api.Models.Responses;

public class LessonProgressResponse
{
    public int Id { get; set; }

    public int LessonId { get; set; }

    public DateTime CompletedAt { get; set; }
}