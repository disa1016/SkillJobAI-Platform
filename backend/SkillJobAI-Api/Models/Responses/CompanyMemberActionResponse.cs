namespace SkillJobAI.Api.Models.Responses;

public class CompanyMemberActionResponse
{
    public string Message { get; set; } = string.Empty;
    public int Id { get; set; }
    public int UserId { get; set; }
    public int CompanyId { get; set; }
    public string Role { get; set; } = string.Empty;
    public DateTime JoinedAt { get; set; }
}