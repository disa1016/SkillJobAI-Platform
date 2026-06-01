namespace SkillJobAI.Api.Entities;

public class Course
{
    public int Id { get; set; }

    public string Title { get; set; } = "";

    public string Description { get; set; } = "";

    public string Category { get; set; } = "";

    public string Level { get; set; } = "Beginner";

    public string Instructor { get; set; } = "";

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public List<Lesson> Lessons { get; set; } = new();

    public List<Enrollment> Enrollments { get; set; } = new();
}