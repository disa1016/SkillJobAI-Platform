using System.ComponentModel.DataAnnotations;

namespace SkillJobAI.Api.Models;

public class LogoutRequest
{
    [Required]
    public string RefreshToken { get; set; } = string.Empty;
}