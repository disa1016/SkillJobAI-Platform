namespace SkillJobAI.Api.Models.Responses;

public class CandidateDashboardResponse
{
    public int ApplicationsCount { get; set; }
    public int EnrollmentsCount { get; set; }
    public int CompletedLessonsCount { get; set; }

    public List<string> UserSkills { get; set; } = new();
    public List<string> MissingSkills { get; set; } = new();

    public List<CandidateRecommendedCourseResponse> RecommendedCourses { get; set; } = new();
    public List<TopJobMatchResponse> TopJobMatches { get; set; } = new();
}

public class CandidateRecommendedCourseResponse
{
    public int Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Skill { get; set; } = string.Empty;
}

public class TopJobMatchResponse
{
    public int Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string? Location { get; set; }
    public string? Salary { get; set; }

    public JobMatchCompanyResponse? Company { get; set; }

    public int MatchPercentage { get; set; }

    public List<string> MatchedSkills { get; set; } = new();
    public List<string> MissingSkills { get; set; } = new();
}

public class JobMatchCompanyResponse
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
}