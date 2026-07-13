namespace SkillJobAI.Api.Models;

public class CreateApplicationRequest
{
    public int JobId { get; set; }

    public string CoverLetter { get; set; } = "";

    public IFormFile? CvFile { get; set; }

    public IFormFile? CertificateFile { get; set; }

    public IFormFile? PortfolioFile { get; set; }
}