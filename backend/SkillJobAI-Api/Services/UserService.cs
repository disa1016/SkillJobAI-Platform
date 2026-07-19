using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using SkillJobAI.Api.Data;
using SkillJobAI.Api.Entities;
using SkillJobAI.Api.Models;
using SkillJobAI.Api.Models.Responses;

namespace SkillJobAI.Api.Services;

public class UserService : IUserService
{
    private const long MaxCvSize =
        5 * 1024 * 1024;

    private const string PrivateCvUploadDirectory =
        "profile-cv";

    private const string CvDownloadUrl =
        "/api/users/cv";

    private const long MaxProfileImageSize =
        10 * 1024 * 1024;

    private const string ProfileImageUploadDirectory =
        "uploads/profile-images";

    private static readonly string[]
        AllowedProfileImageExtensions =
        [
            ".jpg",
            ".jpeg",
            ".png",
            ".webp"
        ];

    private static readonly string[]
        AllowedProfileImageContentTypes =
        [
            "image/jpeg",
            "image/png",
            "image/webp"
        ];

    private readonly AppDbContext _context;
    private readonly IWebHostEnvironment _environment;

    public UserService(
        AppDbContext context,
        IWebHostEnvironment environment
    )
    {
        _context = context;
        _environment = environment;
    }

    public async Task<UserResponse?> GetProfileAsync(
        int userId
    )
    {
        return await _context.Users
            .AsNoTracking()
            .Where(user => user.Id == userId)
            .Select(user => new UserResponse
            {
                Id = user.Id,
                FullName = user.FullName,
                Email = user.Email,
                Role = user.Role,

                // Der interne Speicherpfad darf nicht
                // an das Frontend ausgegeben werden.
                CvUrl = string.IsNullOrWhiteSpace(
                    user.CvUrl)
                    ? string.Empty
                    : CvDownloadUrl,

                ProfileImageUrl =
                    user.ProfileImageUrl
                    ?? string.Empty,

                PhoneNumber =
                    user.PhoneNumber
                    ?? string.Empty,

                Location =
                    user.Location
                    ?? string.Empty,

                Headline =
                    user.Headline
                    ?? string.Empty,

                About =
                    user.About
                    ?? string.Empty,

                LinkedInUrl =
                    user.LinkedInUrl
                    ?? string.Empty,

                GithubUrl =
                    user.GithubUrl
                    ?? string.Empty,

                Website =
                    user.Website
                    ?? string.Empty,

                CreatedAt = user.CreatedAt
            })
            .FirstOrDefaultAsync();
    }

    public async Task<UserResponse?> UpdateProfileAsync(
        int userId,
        UpdateProfileRequest request
    )
    {
        var user =
            await _context.Users.FindAsync(userId);

        if (user is null)
        {
            return null;
        }

        user.FullName =
            request.FullName.Trim();

        user.PhoneNumber =
            NormalizeOptionalValue(
                request.PhoneNumber);

        user.Location =
            NormalizeOptionalValue(
                request.Location);

        user.Headline =
            NormalizeOptionalValue(
                request.Headline);

        user.About =
            NormalizeOptionalValue(
                request.About);

        user.LinkedInUrl =
            NormalizeOptionalValue(
                request.LinkedInUrl);

        user.GithubUrl =
            NormalizeOptionalValue(
                request.GithubUrl);

        user.Website =
            NormalizeOptionalValue(
                request.Website);

        await _context.SaveChangesAsync();

        return MapToResponse(user);
    }

    public async Task<(
        bool Success,
        string? ErrorMessage,
        string? CvUrl
    )> UploadCvAsync(
        int userId,
        IFormFile file
    )
    {
        var user =
            await _context.Users.FindAsync(userId);

        if (user is null)
        {
            return (
                false,
                "User not found.",
                null
            );
        }

        if (file is null || file.Length == 0)
        {
            return (
                false,
                "No file uploaded.",
                null
            );
        }

        if (file.Length > MaxCvSize)
        {
            return (
                false,
                "PDF file must be smaller than 5MB.",
                null
            );
        }

        var extension = Path
            .GetExtension(file.FileName)
            .ToLowerInvariant();

        if (extension != ".pdf")
        {
            return (
                false,
                "Only PDF files are allowed.",
                null
            );
        }

        if (!string.Equals(
                file.ContentType,
                "application/pdf",
                StringComparison.OrdinalIgnoreCase))
        {
            return (
                false,
                "The uploaded file is not a valid PDF.",
                null
            );
        }

        if (!await HasValidPdfSignatureAsync(file))
        {
            return (
                false,
                "The uploaded file is not a valid PDF.",
                null
            );
        }

        var privateCvRoot =
            GetPrivateCvRootPath();

        Directory.CreateDirectory(
            privateCvRoot);

        var fileName =
            $"cv-user-{user.Id}-" +
            $"{Guid.NewGuid():N}.pdf";

        var filePath = Path.Combine(
            privateCvRoot,
            fileName);

        var previousCvStoragePath =
            user.CvUrl;

        // In der Datenbank wird nur ein interner,
        // relativer Speicherpfad abgelegt.
        var newCvStoragePath =
            $"{PrivateCvUploadDirectory}/{fileName}";

        try
        {
            await using var stream =
                new FileStream(
                    filePath,
                    FileMode.CreateNew,
                    FileAccess.Write,
                    FileShare.None,
                    bufferSize: 81920,
                    useAsync: true);

            await file.CopyToAsync(stream);

            user.CvUrl =
                newCvStoragePath;

            await _context.SaveChangesAsync();
        }
        catch
        {
            DeleteFileIfExists(filePath);
            throw;
        }

        DeleteStoredCvFile(
            previousCvStoragePath);

        return (
            true,
            null,
            CvDownloadUrl
        );
    }

    public async Task<(
        string FilePath,
        string DownloadName
    )?> GetCvDownloadAsync(
        int userId
    )
    {
        var storedPath =
            await _context.Users
                .AsNoTracking()
                .Where(user => user.Id == userId)
                .Select(user => user.CvUrl)
                .FirstOrDefaultAsync();

        if (string.IsNullOrWhiteSpace(
                storedPath))
        {
            return null;
        }

        var resolvedFilePath =
            ResolveStoredCvPath(storedPath);

        if (resolvedFilePath is null ||
            !File.Exists(resolvedFilePath))
        {
            return null;
        }

        return (
            resolvedFilePath,
            $"skilljobai-cv-{userId}.pdf"
        );
    }

    public async Task<bool> DeleteCvAsync(
        int userId
    )
    {
        var user =
            await _context.Users.FindAsync(userId);

        if (user is null)
        {
            return false;
        }

        var existingCvStoragePath =
            user.CvUrl;

        user.CvUrl = null;

        await _context.SaveChangesAsync();

        DeleteStoredCvFile(
            existingCvStoragePath);

        return true;
    }

    public async Task<(
        bool Success,
        string? ErrorMessage,
        string? ProfileImageUrl
    )> UploadProfileImageAsync(
        int userId,
        IFormFile file
    )
    {
        var user =
            await _context.Users.FindAsync(userId);

        if (user is null)
        {
            return (
                false,
                "User not found.",
                null
            );
        }

        if (file is null ||
            file.Length == 0)
        {
            return (
                false,
                "No image uploaded.",
                null
            );
        }

        if (file.Length >
            MaxProfileImageSize)
        {
            return (
                false,
                "Profile image must be smaller than 10 MB.",
                null
            );
        }

        var extension = Path
            .GetExtension(file.FileName)
            .ToLowerInvariant();

        if (!AllowedProfileImageExtensions
                .Contains(
                    extension,
                    StringComparer.OrdinalIgnoreCase))
        {
            return (
                false,
                "Only JPG, JPEG, PNG and WEBP images are allowed.",
                null
            );
        }

        if (!AllowedProfileImageContentTypes
                .Contains(
                    file.ContentType,
                    StringComparer.OrdinalIgnoreCase))
        {
            return (
                false,
                "The uploaded file is not a valid image.",
                null
            );
        }

        if (!await ImageFileValidator
                .HasValidSignatureAsync(
                    file,
                    extension))
        {
            return (
                false,
                "The uploaded file is not a valid JPEG, PNG or WEBP image.",
                null
            );
        }

        var webRootPath =
            GetWebRootPath();

        var uploadsFolder =
            Path.Combine(
                webRootPath,
                "uploads",
                "profile-images");

        Directory.CreateDirectory(
            uploadsFolder);

        var normalizedExtension =
            extension == ".jpeg"
                ? ".jpg"
                : extension;

        var fileName =
            $"profile-user-{user.Id}-" +
            $"{Guid.NewGuid():N}" +
            normalizedExtension;

        var filePath = Path.Combine(
            uploadsFolder,
            fileName);

        var previousProfileImageUrl =
            user.ProfileImageUrl;

        var newProfileImageUrl =
            $"/{ProfileImageUploadDirectory}/" +
            fileName;

        try
        {
            await using var stream =
                new FileStream(
                    filePath,
                    FileMode.CreateNew,
                    FileAccess.Write,
                    FileShare.None,
                    bufferSize: 81920,
                    useAsync: true);

            await file.CopyToAsync(stream);

            user.ProfileImageUrl =
                newProfileImageUrl;

            await _context.SaveChangesAsync();
        }
        catch
        {
            DeleteFileIfExists(filePath);
            throw;
        }

        DeletePhysicalPublicFile(
            previousProfileImageUrl);

        return (
            true,
            null,
            newProfileImageUrl
        );
    }

    public async Task<bool>
        DeleteProfileImageAsync(
            int userId
        )
    {
        var user =
            await _context.Users.FindAsync(userId);

        if (user is null)
        {
            return false;
        }

        var existingProfileImageUrl =
            user.ProfileImageUrl;

        user.ProfileImageUrl = null;

        await _context.SaveChangesAsync();

        DeletePhysicalPublicFile(
            existingProfileImageUrl);

        return true;
    }

    public async Task<UserResponse?>
        UpdateUserRoleAsync(
            int id,
            UpdateUserRoleRequest request
        )
    {
        var user =
            await _context.Users.FindAsync(id);

        if (user is null)
        {
            return null;
        }

        user.Role =
            request.Role.Trim();

        await _context.SaveChangesAsync();

        return MapToResponse(user);
    }

    private static string?
        NormalizeOptionalValue(
            string? value
        )
    {
        return string.IsNullOrWhiteSpace(value)
            ? null
            : value.Trim();
    }

    private static UserResponse MapToResponse(
        AppUser user
    )
    {
        return new UserResponse
        {
            Id = user.Id,
            FullName = user.FullName,
            Email = user.Email,
            Role = user.Role,

            CvUrl =
                string.IsNullOrWhiteSpace(
                    user.CvUrl)
                    ? string.Empty
                    : CvDownloadUrl,

            ProfileImageUrl =
                user.ProfileImageUrl
                ?? string.Empty,

            PhoneNumber =
                user.PhoneNumber
                ?? string.Empty,

            Location =
                user.Location
                ?? string.Empty,

            Headline =
                user.Headline
                ?? string.Empty,

            About =
                user.About
                ?? string.Empty,

            LinkedInUrl =
                user.LinkedInUrl
                ?? string.Empty,

            GithubUrl =
                user.GithubUrl
                ?? string.Empty,

            Website =
                user.Website
                ?? string.Empty,

            CreatedAt = user.CreatedAt
        };
    }

    private string GetWebRootPath()
    {
        if (!string.IsNullOrWhiteSpace(
                _environment.WebRootPath))
        {
            return _environment.WebRootPath;
        }

        var webRootPath =
            Path.Combine(
                _environment.ContentRootPath,
                "wwwroot");

        Directory.CreateDirectory(
            webRootPath);

        return webRootPath;
    }

    private string GetPrivateUploadRootPath()
    {
        var privateUploadRoot =
            Path.Combine(
                _environment.ContentRootPath,
                "private_uploads");

        Directory.CreateDirectory(
            privateUploadRoot);

        return privateUploadRoot;
    }

    private string GetPrivateCvRootPath()
    {
        return Path.Combine(
            GetPrivateUploadRootPath(),
            PrivateCvUploadDirectory);
    }

    private string? ResolveStoredCvPath(
        string storedPath
    )
    {
        /*
         * Neue CVs:
         * profile-cv/datei.pdf
         *
         * Alte CVs:
         * /uploads/cv/datei.pdf
         *
         * Alte Dateien werden nur aus Gründen
         * der Rückwärtskompatibilität gelesen.
         */

        if (storedPath.StartsWith(
                "/uploads/cv/",
                StringComparison.OrdinalIgnoreCase) ||
            storedPath.StartsWith(
                "uploads/cv/",
                StringComparison.OrdinalIgnoreCase))
        {
            return ResolveSafePath(
                GetWebRootPath(),
                storedPath);
        }

        return ResolveSafePath(
            GetPrivateUploadRootPath(),
            storedPath);
    }

    private void DeleteStoredCvFile(
        string? storedPath
    )
    {
        if (string.IsNullOrWhiteSpace(
                storedPath))
        {
            return;
        }

        var resolvedFilePath =
            ResolveStoredCvPath(storedPath);

        if (resolvedFilePath is null)
        {
            return;
        }

        DeleteFileIfExists(
            resolvedFilePath);
    }

    private void DeletePhysicalPublicFile(
        string? fileUrl
    )
    {
        if (string.IsNullOrWhiteSpace(
                fileUrl))
        {
            return;
        }

        var filePath =
            ResolveSafePath(
                GetWebRootPath(),
                fileUrl);

        if (filePath is null)
        {
            return;
        }

        DeleteFileIfExists(filePath);
    }

    private static string? ResolveSafePath(
        string allowedRoot,
        string relativePath
    )
    {
        var normalizedRoot =
            Path.GetFullPath(allowedRoot);

        var normalizedRelativePath =
            relativePath
                .TrimStart('/', '\\')
                .Replace(
                    '/',
                    Path.DirectorySeparatorChar)
                .Replace(
                    '\\',
                    Path.DirectorySeparatorChar);

        var fullPath =
            Path.GetFullPath(
                Path.Combine(
                    normalizedRoot,
                    normalizedRelativePath));

        var pathRelativeToRoot =
            Path.GetRelativePath(
                normalizedRoot,
                fullPath);

        var isOutsideRoot =
            pathRelativeToRoot == ".." ||
            pathRelativeToRoot.StartsWith(
                $"..{Path.DirectorySeparatorChar}",
                StringComparison.Ordinal) ||
            Path.IsPathRooted(
                pathRelativeToRoot);

        return isOutsideRoot
            ? null
            : fullPath;
    }

    private static async Task<bool>
        HasValidPdfSignatureAsync(
            IFormFile file
        )
    {
        if (file.Length < 5)
        {
            return false;
        }

        var signature =
            new byte[5];

        await using var stream =
            file.OpenReadStream();

        var bytesRead =
            await stream.ReadAsync(
                signature.AsMemory(
                    0,
                    signature.Length));

        if (bytesRead < signature.Length)
        {
            return false;
        }

        return signature[0] == (byte)'%' &&
               signature[1] == (byte)'P' &&
               signature[2] == (byte)'D' &&
               signature[3] == (byte)'F' &&
               signature[4] == (byte)'-';
    }

    private static void DeleteFileIfExists(
        string filePath
    )
    {
        if (File.Exists(filePath))
        {
            File.Delete(filePath);
        }
    }
}