namespace SkillJobAI.Api.Models.Responses;

public class SkillGapResponse
{
    public int JobId { get; set; }

    public string JobTitle { get; set; } = "";

    public bool HasJobSkills { get; set; }

    public int MatchPercentage { get; set; }

    public List<string> JobSkills { get; set; } = new();

    public List<string> UserSkills { get; set; } = new();

    public List<string> MatchedSkills { get; set; } = new();

    public List<string> MissingSkills { get; set; } = new();

    public List<RecommendedCourseResponse> RecommendedCourses { get; set; } = new();
}

public class RecommendedCourseResponse
{
    public int Id { get; set; }

    public string Title { get; set; } = "";
}