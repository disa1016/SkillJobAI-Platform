namespace SkillJobAI.Api.Services;

public class FileStorageService : IFileStorageService
{
    public async Task<string?> SavePdfFileAsync(IFormFile? file, string folderName, int userId)
    {
        if (file == null || file.Length == 0)
            return null;

        var extension = Path.GetExtension(file.FileName).ToLower();

        if (extension != ".pdf")
            throw new Exception("Only PDF files are allowed.");

        if (file.Length > 5 * 1024 * 1024)
            throw new Exception("PDF file must be smaller than 5MB.");

        var uploadsFolder = Path.Combine(
            Directory.GetCurrentDirectory(),
            "wwwroot",
            "uploads",
            "applications",
            folderName
        );

        if (!Directory.Exists(uploadsFolder))
            Directory.CreateDirectory(uploadsFolder);

        var fileName = $"{folderName}-user-{userId}-{Guid.NewGuid()}.pdf";
        var filePath = Path.Combine(uploadsFolder, fileName);

        await using var stream = new FileStream(filePath, FileMode.Create);
        await file.CopyToAsync(stream);

        return $"/uploads/applications/{folderName}/{fileName}";
    }
}