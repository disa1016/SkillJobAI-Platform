namespace SkillJobAI.Api.Models.Responses;

public class ApplicationFileDownloadResponse
{
    public string FilePath { get; set; } = string.Empty;

    public string DownloadFileName { get; set; } = string.Empty;

    public string ContentType { get; set; } = "application/pdf";
}