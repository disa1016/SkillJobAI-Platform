namespace SkillJobAI.Api.Entities;

public class Job
{
    public int Id { get; set; }

    public string Title { get; set; } = "";

    public string Description { get; set; } = "";

    public string Company { get; set; } = "";

    public string Location { get; set; } = "";

    public string Salary { get; set; } = "";

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}