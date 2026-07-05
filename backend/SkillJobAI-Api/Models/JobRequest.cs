using System.ComponentModel.DataAnnotations;

namespace SkillJobAI.Api.Models;

public class JobRequest
{
    [Required]
    [MaxLength(150)]
    public string Title { get; set; } = "";

    [Required]
    [MaxLength(5000)]
    public string Description { get; set; } = "";

    [Required]
    [MaxLength(150)]
    public string Location { get; set; } = "";

    [MaxLength(100)]
    public string Salary { get; set; } = "";

    [Required]
    public int? CompanyId { get; set; }
}