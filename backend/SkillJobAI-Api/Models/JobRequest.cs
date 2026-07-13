using System.ComponentModel.DataAnnotations;

namespace SkillJobAI.Api.Models;

public class JobRequest
{
    [Required]
    [StringLength(150)]
    public string Title { get; set; } = string.Empty;

    [Required]
    [StringLength(5000)]
    public string Description { get; set; } = string.Empty;

    [Required]
    [StringLength(150)]
    public string Location { get; set; } = string.Empty;

    [StringLength(100)]
    public string Salary { get; set; } = string.Empty;

    [Required]
    public int? CompanyId { get; set; }
}