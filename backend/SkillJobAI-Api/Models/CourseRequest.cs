using System.ComponentModel.DataAnnotations;

namespace SkillJobAI.Api.Models;

public class CourseRequest
{
    [Required]
    [MaxLength(150)]
    public string Title { get; set; } = "";

    [Required]
    [MaxLength(5000)]
    public string Description { get; set; } = "";

    [Required]
    [MaxLength(100)]
    public string Category { get; set; } = "";

    [Required]
    [MaxLength(50)]
    public string Level { get; set; } = "Beginner";

    [Required]
    [MaxLength(150)]
    public string Instructor { get; set; } = "";
}