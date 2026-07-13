namespace SkillJobAI.Api.Models.Responses;

public class UserResponse
{
    public int Id { get; set; }

    public string FullName { get; set; } = "";

    public string Email { get; set; } = "";

    public string Role { get; set; } = "";

    public string CvUrl { get; set; } = "";

    public DateTime CreatedAt { get; set; }
}