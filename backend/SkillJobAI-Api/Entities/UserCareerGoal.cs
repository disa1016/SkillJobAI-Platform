namespace SkillJobAI.Api.Entities;

public class UserCareerGoal
{
    public int Id { get; set; }

    public int UserId { get; set; }

    public AppUser User { get; set; } = null!;

    public int CareerGoalId { get; set; }

    public CareerGoal CareerGoal { get; set; } = null!;

    public DateTime StartedAt { get; set; } = DateTime.UtcNow;
}