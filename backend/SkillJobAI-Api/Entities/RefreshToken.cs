namespace SkillJobAI.Api.Entities;

public class RefreshToken
{
    public int Id { get; set; }

    public int UserId { get; set; }
    public AppUser User { get; set; } = null!;

    // In der Datenbank wird nur der SHA-256-Hash gespeichert.
    public string TokenHash { get; set; } = string.Empty;

    public DateTime CreatedAt { get; set; }
    public DateTime ExpiresAt { get; set; }

    public DateTime? RevokedAt { get; set; }

    // Hash des Tokens, der diesen Token ersetzt hat.
    public string? ReplacedByTokenHash { get; set; }
}