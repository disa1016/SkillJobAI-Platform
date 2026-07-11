using System.ComponentModel.DataAnnotations;

namespace SkillJobAI.Api.Models;

public class RefreshTokenRequest
{
    [Required]
    public string RefreshToken { get; set; } = string.Empty;
}