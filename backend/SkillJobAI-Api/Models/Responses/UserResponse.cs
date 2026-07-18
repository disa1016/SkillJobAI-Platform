namespace SkillJobAI.Api.Models.Responses;

public class UserResponse
{
    public int Id { get; set; }

    public string FullName { get; set; } = string.Empty;

    public string Email { get; set; } = string.Empty;

    public string Role { get; set; } = string.Empty;

    public string CvUrl { get; set; } = string.Empty;

    public string PhoneNumber { get; set; } = string.Empty;

    public string Location { get; set; } = string.Empty;

    public string Headline { get; set; } = string.Empty;

    public string About { get; set; } = string.Empty;

    public string LinkedInUrl { get; set; } = string.Empty;

    public string GithubUrl { get; set; } = string.Empty;

    public string Website { get; set; } = string.Empty;

    public DateTime CreatedAt { get; set; }
}