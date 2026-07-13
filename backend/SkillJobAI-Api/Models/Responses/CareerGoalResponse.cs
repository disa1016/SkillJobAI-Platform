namespace SkillJobAI.Api.Models.Responses;

public class CareerGoalResponse
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public int DurationMonths { get; set; }
}