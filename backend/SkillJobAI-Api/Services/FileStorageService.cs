namespace SkillJobAI.Api.Services;

public class FileStorageService : IFileStorageService
{
    public async Task<string?> SavePdfFileAsync(
        IFormFile? file,
        string folderName,
        int userId)
    {
        if (file == null || file.Length == 0)
        {
            return null;
        }

        var extension = Path
            .GetExtension(file.FileName)
            .ToLowerInvariant();

        if (extension != ".pdf")
        {
            throw new InvalidOperationException(
                "Only PDF files are allowed.");
        }

        if (file.Length > 5 * 1024 * 1024)
        {
            throw new InvalidOperationException(
                "PDF file must be smaller than 5MB.");
        }

        var allowedFolders = new[]
        {
            "cv",
            "certificates",
            "portfolio"
        };

        var normalizedFolderName = folderName
            .Trim()
            .ToLowerInvariant();

        if (!allowedFolders.Contains(normalizedFolderName))
        {
            throw new InvalidOperationException(
                "Invalid upload folder.");
        }

        var uploadsFolder = Path.Combine(
            Directory.GetCurrentDirectory(),
            "private_uploads",
            "applications",
            normalizedFolderName);

        Directory.CreateDirectory(uploadsFolder);

        var fileName =
            $"{normalizedFolderName}-user-{userId}-{Guid.NewGuid():N}.pdf";

        var filePath = Path.Combine(
            uploadsFolder,
            fileName);

        await using var stream = new FileStream(
            filePath,
            FileMode.CreateNew,
            FileAccess.Write,
            FileShare.None);

        await file.CopyToAsync(stream);

        return Path.Combine(
                "private_uploads",
                "applications",
                normalizedFolderName,
                fileName)
            .Replace(
                Path.DirectorySeparatorChar,
                '/');
    }
}