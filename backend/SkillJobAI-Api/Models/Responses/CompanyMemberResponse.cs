namespace SkillJobAI.Api.Models.Responses;

public class CompanyMemberResponse
{
    public int Id { get; set; }
    public int UserId { get; set; }
    public int CompanyId { get; set; }
    public string Role { get; set; } = string.Empty;
    public DateTime JoinedAt { get; set; }

    public RecruiterInfoResponse Recruiter { get; set; } = new();
    public CompanyInfoResponse Company { get; set; } = new();
}

public class RecruiterInfoResponse
{
    public int Id { get; set; }
    public string? FullName { get; set; }
    public string? Email { get; set; }
}

public class CompanyInfoResponse
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Location { get; set; }
}