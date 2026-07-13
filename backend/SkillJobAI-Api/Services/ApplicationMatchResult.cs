namespace SkillJobAI.Api.Services;

public class ApplicationMatchResult
{
    public int MatchPercentage { get; set; }

    public List<string> JobSkills { get; set; } = new();

    public List<string> UserSkills { get; set; } = new();

    public List<string> MatchedSkills { get; set; } = new();

    public List<string> MissingSkills { get; set; } = new();

    public List<object> RecommendedCourses { get; set; } = new();
}