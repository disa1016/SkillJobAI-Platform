namespace SkillJobAI.Api.Services;

public interface ICertificateService
{
    Task<(bool Success, string? ErrorMessage, byte[]? PdfBytes, string? FileName)> GenerateCourseCertificateAsync(
        int userId,
        int courseId);
}