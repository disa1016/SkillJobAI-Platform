using System.ComponentModel.DataAnnotations;
using SkillJobAI.Api.Constants;

namespace SkillJobAI.Api.Entities;

public class AppUser
{
    public int Id { get; set; }

    [Required]
    [StringLength(150)]
    public string FullName { get; set; } = string.Empty;

    [Required]
    [EmailAddress]
    [StringLength(255)]
    public string Email { get; set; } = string.Empty;

    [Required]
    public string PasswordHash { get; set; } = string.Empty;

    [Required]
    [StringLength(50)]
    public string Role { get; set; } = AppRoles.Candidate;

    [StringLength(500)]
    public string? CvUrl { get; set; }

    [Phone]
    [StringLength(50)]
    public string? PhoneNumber { get; set; }

    [StringLength(150)]
    public string? Location { get; set; }

    [StringLength(200)]
    public string? Headline { get; set; }

    [StringLength(2000)]
    public string? About { get; set; }

    [Url]
    [StringLength(500)]
    public string? LinkedInUrl { get; set; }

    [Url]
    [StringLength(500)]
    public string? GithubUrl { get; set; }

    [Url]
    [StringLength(500)]
    public string? Website { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public ICollection<RefreshToken> RefreshTokens { get; set; } =
        new List<RefreshToken>();
}