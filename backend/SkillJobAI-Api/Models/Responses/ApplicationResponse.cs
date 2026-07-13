namespace SkillJobAI.Api.Models.Responses;

public class ApplicationResponse
{
    public int Id { get; set; }

    public int UserId { get; set; }

    public int JobId { get; set; }

    public string CoverLetter { get; set; } = "";

    public string Status { get; set; } = "";

    public string CvFileUrl { get; set; } = "";

    public string CertificateFileUrl { get; set; } = "";

    public string PortfolioFileUrl { get; set; } = "";

    public DateTime CreatedAt { get; set; }

    public ApplicationCandidateResponse? Candidate { get; set; }

    public ApplicationJobResponse? Job { get; set; }

    public int MatchPercentage { get; set; }

    public List<string> JobSkills { get; set; } = new();

    public List<string> UserSkills { get; set; } = new();

    public List<string> MatchedSkills { get; set; } = new();

    public List<string> MissingSkills { get; set; } = new();

    public object RecommendedCourses { get; set; } = new();
}

public class ApplicationCandidateResponse
{
    public int Id { get; set; }

    public string FullName { get; set; } = "";

    public string Email { get; set; } = "";

    public string CvUrl { get; set; } = "";
}

public class ApplicationJobResponse
{
    public int Id { get; set; }

    public string Title { get; set; } = "";

    public string Company { get; set; } = "";

    public int? CompanyId { get; set; }

    public string Location { get; set; } = "";

    public string Salary { get; set; } = "";
}