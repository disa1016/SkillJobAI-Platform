using System.ComponentModel.DataAnnotations;

namespace SkillJobAI.Api.Models;

public class CompanyRequest
{
    [Required]
    [StringLength(100)]
    public string Name { get; set; } = string.Empty;

    [StringLength(1000)]
    public string? Description { get; set; }

    [Url]
    public string? WebsiteUrl { get; set; }

    [Url]
    public string? LogoUrl { get; set; }

    [StringLength(100)]
    public string? Location { get; set; }
}