namespace SkillJobAI.Api.Models;

public class AnalyzeCvRequest
{
    public string CvText { get; set; } = string.Empty;
}

public class JobMatchRequest
{
    public string CvText { get; set; } = string.Empty;
    public string JobDescription { get; set; } = string.Empty;
}

public class JobRecommendationsRequest
{
    public string CvText { get; set; } = string.Empty;
}

public class CoverLetterRequest
{
    public string FullName { get; set; } = string.Empty;
    public string Company { get; set; } = string.Empty;
    public string JobTitle { get; set; } = string.Empty;
    public string CvSummary { get; set; } = string.Empty;
}