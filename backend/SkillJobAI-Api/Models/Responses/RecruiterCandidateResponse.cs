namespace SkillJobAI.Api.Models.Responses;

public class RecruiterCandidateResponse
{
    public int Id { get; set; }
    public string? FullName { get; set; }
    public string Email { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }

    public List<string> Skills { get; set; } = new();
    public int SkillsCount { get; set; }
    public int ApplicationsCount { get; set; }
    public int AcceptedApplications { get; set; }
    public int RejectedApplications { get; set; }
}