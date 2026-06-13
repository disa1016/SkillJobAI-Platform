namespace SkillJobAI.Api.Models;

public class CompanyRequest
{
    public string Name { get; set; } = "";

    public string? Description { get; set; }

    public string? WebsiteUrl { get; set; }

    public string? LogoUrl { get; set; }

    public string? Location { get; set; }
}