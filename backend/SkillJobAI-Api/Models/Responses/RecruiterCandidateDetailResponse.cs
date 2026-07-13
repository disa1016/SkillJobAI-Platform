namespace SkillJobAI.Api.Models.Responses;

public class RecruiterCandidateDetailResponse
{
    public int Id { get; set; }
    public string? FullName { get; set; }
    public string Email { get; set; } = string.Empty;
    public string? CvUrl { get; set; }
    public DateTime CreatedAt { get; set; }

    public List<string> Skills { get; set; } = new();
    public int SkillsCount { get; set; }
    public int ApplicationsCount { get; set; }
    public int AcceptedApplications { get; set; }
    public int RejectedApplications { get; set; }

    public List<RecruiterCandidateApplicationResponse> Applications { get; set; } = new();
}

public class RecruiterCandidateApplicationResponse
{
    public int Id { get; set; }
    public int JobId { get; set; }
    public string? CoverLetter { get; set; }
    public string Status { get; set; } = string.Empty;
    public string? CvFileUrl { get; set; }
    public string? CertificateFileUrl { get; set; }
    public string? PortfolioFileUrl { get; set; }
    public DateTime CreatedAt { get; set; }

    public RecruiterCandidateJobResponse? Job { get; set; }
}

public class RecruiterCandidateJobResponse
{
    public int Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string? Company { get; set; }
    public string? Location { get; set; }
    public string? Salary { get; set; }
}