namespace SkillJobAI.Api.Entities;

public class CareerGoalSkill
{
    public int Id { get; set; }

    public int CareerGoalId { get; set; }

    public CareerGoal CareerGoal { get; set; } = null!;

    public int SkillId { get; set; }

    public Skill Skill { get; set; } = null!;

    public int MonthNumber { get; set; }
}