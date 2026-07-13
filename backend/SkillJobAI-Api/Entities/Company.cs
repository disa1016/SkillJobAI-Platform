using System.ComponentModel.DataAnnotations.Schema;

namespace SkillJobAI.Api.Entities;

[Table("Company")]
public class Company
{
    public int Id { get; set; }

    public string Name { get; set; } = "";

    public string? Description { get; set; }

    public string? WebsiteUrl { get; set; }

    public string? LogoUrl { get; set; }

    public string? Location { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public ICollection<Job> Jobs { get; set; } = new List<Job>();

    public ICollection<CompanyMember> Members { get; set; } = new List<CompanyMember>();
}