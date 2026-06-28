namespace SkillJobAI.Api.Services;

public interface IFileStorageService
{
    Task<string?> SavePdfFileAsync(IFormFile? file, string folderName, int userId);
}