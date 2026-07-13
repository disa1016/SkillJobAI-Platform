namespace SkillJobAI.Api.Models.Responses;

public class AnalyzeCvResponse
{
    public int Score { get; set; }
    public List<string> Skills { get; set; } = new();
    public List<SkillCategoryResponse> SkillCategories { get; set; } = new();
    public List<string> Suggestions { get; set; } = new();
}

public class AnalyzeCvPdfResponse
{
    public string FileName { get; set; } = string.Empty;
    public string ExtractedText { get; set; } = string.Empty;
    public int Score { get; set; }
    public List<string> Skills { get; set; } = new();
    public List<SkillCategoryResponse> SkillCategories { get; set; } = new();
    public List<string> Suggestions { get; set; } = new();
}

public class SkillCategoryResponse
{
    public string Name { get; set; } = string.Empty;
    public List<string> MatchedSkills { get; set; } = new();
    public List<string> MissingSkills { get; set; } = new();
}

public class JobMatchResponse
{
    public int MatchScore { get; set; }
    public List<string> MatchedSkills { get; set; } = new();
    public List<string> MissingSkills { get; set; } = new();
    public string Recommendation { get; set; } = string.Empty;
}

public class JobRecommendationResponse
{
    public int JobId { get; set; }
    public string Title { get; set; } = string.Empty;
    public object? Company { get; set; }
    public string? Location { get; set; }
    public string? Salary { get; set; }
    public string? Description { get; set; }
    public int MatchScore { get; set; }
    public List<string> MatchedSkills { get; set; } = new();
    public List<string> MissingSkills { get; set; } = new();
    public string Recommendation { get; set; } = string.Empty;
}

public class CoverLetterResponse
{
    public string CoverLetter { get; set; } = string.Empty;
}