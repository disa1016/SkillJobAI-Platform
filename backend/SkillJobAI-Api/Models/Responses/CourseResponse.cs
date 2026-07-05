namespace SkillJobAI.Api.Models.Responses;

public class CourseResponse
{
    public int Id { get; set; }

    public string Title { get; set; } = "";

    public string Description { get; set; } = "";

    public string Category { get; set; } = "";

    public string Level { get; set; } = "";

    public string Instructor { get; set; } = "";

    public DateTime CreatedAt { get; set; }

    public List<LessonResponse> Lessons { get; set; } = [];
}