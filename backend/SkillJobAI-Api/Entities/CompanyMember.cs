namespace SkillJobAI.Api.Entities;

public class CompanyMember
{
    public int Id { get; set; }

    public int UserId { get; set; }

    public AppUser User { get; set; } = null!;

    public int CompanyId { get; set; }

    public Company Company { get; set; } = null!;

    public string Role { get; set; } = "Recruiter";

    public DateTime JoinedAt { get; set; } = DateTime.UtcNow;
}