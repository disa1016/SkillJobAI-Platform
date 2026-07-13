namespace SkillJobAI.Api.Models.Responses;

public class PasswordResetResponse
{
    public bool Success { get; set; }
    public string Message { get; set; } = string.Empty;
}