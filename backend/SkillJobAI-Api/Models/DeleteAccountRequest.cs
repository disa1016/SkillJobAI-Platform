using System.ComponentModel.DataAnnotations;

namespace SkillJobAI.Api.Models;

public class DeleteAccountRequest
{
    [Required]
    [MinLength(1)]
    public string Password { get; set; } = string.Empty;

    [Required]
    public bool ConfirmDeletion { get; set; }
}