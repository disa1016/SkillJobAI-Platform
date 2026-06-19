namespace SkillJobAI.Api.Entities;

public class CareerGoal
{
    public int Id { get; set; }

    public string Name { get; set; } = "";

    public string Description { get; set; } = "";

    public int DurationMonths { get; set; } = 8;
}