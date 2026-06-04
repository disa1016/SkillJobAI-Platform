namespace SkillJobAI.Api.Entities;

public class CourseSkill
{
    public int Id { get; set; }

    public int CourseId { get; set; }

    public Course Course { get; set; } = null!;

    public int SkillId { get; set; }

    public Skill Skill { get; set; } = null!;
}