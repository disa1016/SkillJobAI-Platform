using System.ComponentModel.DataAnnotations;

namespace SkillJobAI.Api.Models;

public class LessonProgressRequest
{
    [Required]
    public int LessonId { get; set; }
}