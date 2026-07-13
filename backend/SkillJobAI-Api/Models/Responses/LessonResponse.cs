namespace SkillJobAI.Api.Models.Responses;

public class LessonResponse
{
    public int Id { get; set; }

    public int CourseId { get; set; }

    public string Title { get; set; } = "";

    public string Content { get; set; } = "";

    public string VideoUrl { get; set; } = "";

    public int OrderNumber { get; set; }

    public DateTime CreatedAt { get; set; }
}