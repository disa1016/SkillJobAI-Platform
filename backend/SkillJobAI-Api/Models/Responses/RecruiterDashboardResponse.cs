namespace SkillJobAI.Api.Models.Responses;

public class RecruiterDashboardResponse
{
    public int TotalCompanies { get; set; }
    public int TotalJobs { get; set; }
    public int TotalApplications { get; set; }

    public int PendingApplications { get; set; }
    public int ReviewedApplications { get; set; }
    public int AcceptedApplications { get; set; }
    public int RejectedApplications { get; set; }

    public List<RecruiterRecentApplicationResponse> RecentApplications { get; set; } = new();
    public List<RecruiterTopJobResponse> TopJobsByApplications { get; set; } = new();
}

public class RecruiterRecentApplicationResponse
{
    public int Id { get; set; }
    public string Status { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public string? CoverLetter { get; set; }

    public RecruiterJobInfoResponse? Job { get; set; }
    public RecruiterCandidateInfoResponse? Candidate { get; set; }
}

public class RecruiterTopJobResponse
{
    public int JobId { get; set; }
    public int ApplicationsCount { get; set; }
    public RecruiterJobInfoResponse? Job { get; set; }
}

public class RecruiterJobInfoResponse
{
    public int Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string? Company { get; set; }
}

public class RecruiterCandidateInfoResponse
{
    public int Id { get; set; }
    public string? FullName { get; set; }
    public string Email { get; set; } = string.Empty;
}