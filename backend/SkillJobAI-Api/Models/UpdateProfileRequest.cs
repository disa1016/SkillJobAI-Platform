using System.ComponentModel.DataAnnotations;

namespace SkillJobAI.Api.Models;

public class UpdateProfileRequest
{
    [Required(ErrorMessage = "Full name is required.")]
    [StringLength(
        150,
        MinimumLength = 2,
        ErrorMessage = "Full name must contain between 2 and 150 characters."
    )]
    public string FullName { get; set; } = string.Empty;

    [Phone(ErrorMessage = "Phone number is invalid.")]
    [StringLength(
        50,
        ErrorMessage = "Phone number must not exceed 50 characters."
    )]
    public string? PhoneNumber { get; set; }

    [StringLength(
        150,
        ErrorMessage = "Location must not exceed 150 characters."
    )]
    public string? Location { get; set; }

    [StringLength(
        200,
        ErrorMessage = "Headline must not exceed 200 characters."
    )]
    public string? Headline { get; set; }

    [StringLength(
        2000,
        ErrorMessage = "About text must not exceed 2000 characters."
    )]
    public string? About { get; set; }

    [Url(ErrorMessage = "LinkedIn URL is invalid.")]
    [StringLength(
        500,
        ErrorMessage = "LinkedIn URL must not exceed 500 characters."
    )]
    public string? LinkedInUrl { get; set; }

    [Url(ErrorMessage = "GitHub URL is invalid.")]
    [StringLength(
        500,
        ErrorMessage = "GitHub URL must not exceed 500 characters."
    )]
    public string? GithubUrl { get; set; }

    [Url(ErrorMessage = "Website URL is invalid.")]
    [StringLength(
        500,
        ErrorMessage = "Website URL must not exceed 500 characters."
    )]
    public string? Website { get; set; }
}