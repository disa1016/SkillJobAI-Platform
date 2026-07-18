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
    private const long MaxCvSize = 5 * 1024 * 1024;
    private const string CvUploadDirectory = "uploads/cv";

    private const long MaxProfileImageSize = 10 * 1024 * 1024;
    private const string ProfileImageUploadDirectory =
        "uploads/profile-images";

    private static readonly string[] AllowedProfileImageExtensions =
    {
        ".jpg",
        ".jpeg",
        ".png"
    };

    private static readonly string[] AllowedProfileImageContentTypes =
    {
        "image/jpeg",
        "image/png"
    };

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

    public async Task<UserResponse?> GetProfileAsync(int userId)
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
                CvUrl = user.CvUrl ?? string.Empty,
                ProfileImageUrl =
                    user.ProfileImageUrl ?? string.Empty,
                PhoneNumber =
                    user.PhoneNumber ?? string.Empty,
                Location =
                    user.Location ?? string.Empty,
                Headline =
                    user.Headline ?? string.Empty,
                About =
                    user.About ?? string.Empty,
                LinkedInUrl =
                    user.LinkedInUrl ?? string.Empty,
                GithubUrl =
                    user.GithubUrl ?? string.Empty,
                Website =
                    user.Website ?? string.Empty,
                CreatedAt = user.CreatedAt
            })
            .FirstOrDefaultAsync();
    }

    public async Task<UserResponse?> UpdateProfileAsync(
        int userId,
        UpdateProfileRequest request
    )
    {
        var user = await _context.Users.FindAsync(userId);

        if (user is null)
        {
            return null;
        }

        user.FullName = request.FullName.Trim();

        user.PhoneNumber = NormalizeOptionalValue(
            request.PhoneNumber
        );

        user.Location = NormalizeOptionalValue(
            request.Location
        );

        user.Headline = NormalizeOptionalValue(
            request.Headline
        );

        user.About = NormalizeOptionalValue(
            request.About
        );

        user.LinkedInUrl = NormalizeOptionalValue(
            request.LinkedInUrl
        );

        user.GithubUrl = NormalizeOptionalValue(
            request.GithubUrl
        );

        user.Website = NormalizeOptionalValue(
            request.Website
        );

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
        var user = await _context.Users.FindAsync(userId);

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

        var webRootPath = GetWebRootPath();

        var uploadsFolder = Path.Combine(
            webRootPath,
            "uploads",
            "cv"
        );

        Directory.CreateDirectory(uploadsFolder);

        var fileName =
            $"cv-user-{user.Id}-{Guid.NewGuid():N}.pdf";

        var filePath = Path.Combine(
            uploadsFolder,
            fileName
        );

        var previousCvUrl = user.CvUrl;

        var newCvUrl =
            $"/{CvUploadDirectory}/{fileName}";

        try
        {
            await using var stream = new FileStream(
                filePath,
                FileMode.CreateNew,
                FileAccess.Write,
                FileShare.None,
                bufferSize: 81920,
                useAsync: true
            );

            await file.CopyToAsync(stream);

            user.CvUrl = newCvUrl;

            await _context.SaveChangesAsync();
        }
        catch
        {
            DeleteFileIfExists(filePath);
            throw;
        }

        DeletePhysicalUploadedFile(previousCvUrl);

        return (
            true,
            null,
            newCvUrl
        );
    }

    public async Task<bool> DeleteCvAsync(int userId)
    {
        var user = await _context.Users.FindAsync(userId);

        if (user is null)
        {
            return false;
        }

        var existingCvUrl = user.CvUrl;

        user.CvUrl = null;

        await _context.SaveChangesAsync();

        DeletePhysicalUploadedFile(existingCvUrl);

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
        var user = await _context.Users.FindAsync(userId);

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
                "No image uploaded.",
                null
            );
        }

        if (file.Length > MaxProfileImageSize)
        {
            return (
                false,
                "Profile image must be smaller than 2MB.",
                null
            );
        }

        var extension = Path
            .GetExtension(file.FileName)
            .ToLowerInvariant();

        if (!AllowedProfileImageExtensions.Contains(
                extension,
                StringComparer.OrdinalIgnoreCase
            ))
        {
            return (
                false,
                "Only JPG, JPEG and PNG images are allowed.",
                null
            );
        }

        if (!AllowedProfileImageContentTypes.Contains(
                file.ContentType,
                StringComparer.OrdinalIgnoreCase
            ))
        {
            return (
                false,
                "The uploaded file is not a valid image.",
                null
            );
        }

        var webRootPath = GetWebRootPath();

        var uploadsFolder = Path.Combine(
            webRootPath,
            "uploads",
            "profile-images"
        );

        Directory.CreateDirectory(uploadsFolder);

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
            fileName
        );

        var previousProfileImageUrl =
            user.ProfileImageUrl;

        var newProfileImageUrl =
            $"/{ProfileImageUploadDirectory}/{fileName}";

        try
        {
            await using var stream = new FileStream(
                filePath,
                FileMode.CreateNew,
                FileAccess.Write,
                FileShare.None,
                bufferSize: 81920,
                useAsync: true
            );

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

        DeletePhysicalUploadedFile(
            previousProfileImageUrl
        );

        return (
            true,
            null,
            newProfileImageUrl
        );
    }

    public async Task<bool> DeleteProfileImageAsync(
        int userId
    )
    {
        var user = await _context.Users.FindAsync(userId);

        if (user is null)
        {
            return false;
        }

        var existingProfileImageUrl =
            user.ProfileImageUrl;

        user.ProfileImageUrl = null;

        await _context.SaveChangesAsync();

        DeletePhysicalUploadedFile(
            existingProfileImageUrl
        );

        return true;
    }

    public async Task<UserResponse?> UpdateUserRoleAsync(
        int id,
        UpdateUserRoleRequest request
    )
    {
        var user = await _context.Users.FindAsync(id);

        if (user is null)
        {
            return null;
        }

        user.Role = request.Role.Trim();

        await _context.SaveChangesAsync();

        return MapToResponse(user);
    }

    private static string? NormalizeOptionalValue(
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
            CvUrl = user.CvUrl ?? string.Empty,
            ProfileImageUrl =
                user.ProfileImageUrl ?? string.Empty,
            PhoneNumber =
                user.PhoneNumber ?? string.Empty,
            Location =
                user.Location ?? string.Empty,
            Headline =
                user.Headline ?? string.Empty,
            About =
                user.About ?? string.Empty,
            LinkedInUrl =
                user.LinkedInUrl ?? string.Empty,
            GithubUrl =
                user.GithubUrl ?? string.Empty,
            Website =
                user.Website ?? string.Empty,
            CreatedAt = user.CreatedAt
        };
    }

    private string GetWebRootPath()
    {
        if (!string.IsNullOrWhiteSpace(
                _environment.WebRootPath
            ))
        {
            return _environment.WebRootPath;
        }

        var webRootPath = Path.Combine(
            _environment.ContentRootPath,
            "wwwroot"
        );

        Directory.CreateDirectory(webRootPath);

        return webRootPath;
    }

    private void DeletePhysicalUploadedFile(
        string? fileUrl
    )
    {
        if (string.IsNullOrWhiteSpace(fileUrl))
        {
            return;
        }

        var webRootPath = Path.GetFullPath(
            GetWebRootPath()
        );

        var relativePath = fileUrl
            .TrimStart('/', '\\')
            .Replace(
                '/',
                Path.DirectorySeparatorChar
            )
            .Replace(
                '\\',
                Path.DirectorySeparatorChar
            );

        var filePath = Path.GetFullPath(
            Path.Combine(
                webRootPath,
                relativePath
            )
        );

        var pathRelativeToWebRoot =
            Path.GetRelativePath(
                webRootPath,
                filePath
            );

        var isOutsideWebRoot =
            pathRelativeToWebRoot == ".." ||
            pathRelativeToWebRoot.StartsWith(
                $"..{Path.DirectorySeparatorChar}",
                StringComparison.Ordinal
            ) ||
            Path.IsPathRooted(
                pathRelativeToWebRoot
            );

        if (isOutsideWebRoot)
        {
            return;
        }

        DeleteFileIfExists(filePath);
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