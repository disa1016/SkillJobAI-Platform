using System.ComponentModel.DataAnnotations;

namespace SkillJobAI.Api.Models;

public class EnrollmentRequest
{
    [Required]
    public int CourseId { get; set; }
}