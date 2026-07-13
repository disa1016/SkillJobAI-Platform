namespace SkillJobAI.Api.Models;

public class CompanyMemberRequest
{
    public int UserId { get; set; }

    public int CompanyId { get; set; }

    public string Role { get; set; } = "Recruiter";
}