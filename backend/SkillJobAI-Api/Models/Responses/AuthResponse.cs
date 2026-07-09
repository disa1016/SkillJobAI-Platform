namespace SkillJobAI.Api.Models.Responses;

public class AuthResponse
{
    public string Message { get; set; } = string.Empty;
    public string Token { get; set; } = string.Empty;
    public AuthUserResponse User { get; set; } = new();
}

public class AuthUserResponse
{
    public int Id { get; set; }
    public string FullName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Role { get; set; } = string.Empty;
}