namespace SkillJobAI.Api.Models.Responses;

public class ProfileImageUploadResult
{
    public bool Success { get; set; }

    public bool UserNotFound { get; set; }

    public string? ProfileImageUrl { get; set; }

    public string? ErrorMessage { get; set; }
}