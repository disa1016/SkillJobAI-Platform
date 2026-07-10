using System.ComponentModel.DataAnnotations;

namespace SkillJobAI.Api.Models;

public class UpdateUserRoleRequest
{
    [Required]
    [RegularExpression("^(Candidate|Student|Recruiter|Admin)$",
        ErrorMessage = "Invalid role.")]
    public string Role { get; set; } = string.Empty;
}