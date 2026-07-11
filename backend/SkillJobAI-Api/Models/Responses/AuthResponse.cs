namespace SkillJobAI.Api.Models.Responses;

public class AuthResponse
{
    public string Message { get; set; } = string.Empty;

    // Bestehender Name bleibt erhalten, damit das Frontend nicht kaputtgeht.
    public string Token { get; set; } = string.Empty;

    public string RefreshToken { get; set; } = string.Empty;

    public DateTime TokenExpiresAt { get; set; }

    public AuthUserResponse User { get; set; } = new();
}

public class AuthUserResponse
{
    public int Id { get; set; }
    public string FullName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Role { get; set; } = string.Empty;
}