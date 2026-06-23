namespace SkillJobAI.Api.Entities;

public class Application
{
    public int Id { get; set; }

    public int UserId { get; set; }

    public int JobId { get; set; }

    public string CoverLetter { get; set; } = "";

    public string Status { get; set; } = "Pending";

    public string? CvFileUrl { get; set; }

    public string? CertificateFileUrl { get; set; }

    public string? PortfolioFileUrl { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}