using System.ComponentModel.DataAnnotations;

namespace SkillJobAI.Api.Models;

public class LessonRequest
{
    [Required]
    [StringLength(150)]
    public string Title { get; set; } = "";

    [Required]
    public string Content { get; set; } = "";

    public string? VideoUrl { get; set; }

    [Range(1, 1000)]
    public int OrderNumber { get; set; }

    [Required]
    public int CourseId { get; set; }
}