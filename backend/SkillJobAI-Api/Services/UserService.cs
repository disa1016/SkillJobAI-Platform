using System.IO.Compression;
using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using SkillJobAI.Api.Constants;
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
    private readonly PasswordService _passwordService;
    private readonly ILogger<UserService> _logger;

    public UserService(
        AppDbContext context,
        IWebHostEnvironment environment,
        PasswordService passwordService,
        ILogger<UserService> logger
    )
    {
        _context = context;
        _environment = environment;
        _passwordService = passwordService;
        _logger = logger;
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

    public async Task<(
        bool Success,
        string? ErrorMessage,
        byte[]? ZipBytes,
        string? DownloadName
    )> ExportPersonalDataAsync(
        int userId
    )
    {
        var user =
            await _context.Users
                .AsNoTracking()
                .FirstOrDefaultAsync(
                    existingUser =>
                        existingUser.Id == userId);

        if (user is null)
        {
            return (
                false,
                "User not found.",
                null,
                null
            );
        }

        var applications =
            await (
                from application
                    in _context.Applications

                join job
                    in _context.Jobs
                    on application.JobId
                    equals job.Id

                where application.UserId == userId

                orderby application.CreatedAt
                    descending

                select new
                {
                    application.Id,
                    application.JobId,

                    JobTitle =
                        job.Title,

                    CompanyId =
                        job.CompanyId,

                    JobLocation =
                        job.Location,

                    application.CoverLetter,
                    application.Status,
                    application.CreatedAt,

                    HasCvFile =
                        !string.IsNullOrWhiteSpace(
                            application.CvFileUrl),

                    HasCertificateFile =
                        !string.IsNullOrWhiteSpace(
                            application.CertificateFileUrl),

                    HasPortfolioFile =
                        !string.IsNullOrWhiteSpace(
                            application.PortfolioFileUrl)
                }
            )
            .AsNoTracking()
            .ToListAsync();

        var skills =
            await (
                from userSkill
                    in _context.UserSkills

                join skill
                    in _context.Skills
                    on userSkill.SkillId
                    equals skill.Id

                where userSkill.UserId == userId

                orderby skill.Name

                select new
                {
                    skill.Id,
                    skill.Name
                }
            )
            .AsNoTracking()
            .ToListAsync();

        var enrollments =
            await (
                from enrollment
                    in _context.Enrollments

                join course
                    in _context.Courses
                    on enrollment.CourseId
                    equals course.Id

                where enrollment.UserId == userId

                orderby enrollment.EnrolledAt
                    descending

                select new
                {
                    enrollment.Id,
                    enrollment.CourseId,

                    CourseTitle =
                        course.Title,

                    enrollment.EnrolledAt,
                    enrollment.IsCompleted
                }
            )
            .AsNoTracking()
            .ToListAsync();

        var lessonProgress =
            await (
                from progress
                    in _context.LessonProgresses

                join lesson
                    in _context.Lessons
                    on progress.LessonId
                    equals lesson.Id

                where progress.UserId == userId

                orderby progress.CompletedAt
                    descending

                select new
                {
                    progress.Id,
                    progress.LessonId,

                    LessonTitle =
                        lesson.Title,

                    progress.CompletedAt
                }
            )
            .AsNoTracking()
            .ToListAsync();

        var careerGoals =
            await (
                from userCareerGoal
                    in _context.UserCareerGoals

                join careerGoal
                    in _context.CareerGoals
                    on userCareerGoal.CareerGoalId
                    equals careerGoal.Id

                where userCareerGoal.UserId == userId

                orderby userCareerGoal.StartedAt
                    descending

                select new
                {
                    userCareerGoal.Id,
                    userCareerGoal.CareerGoalId,
                    careerGoal.Name,
                    careerGoal.Description,
                    careerGoal.DurationMonths,
                    userCareerGoal.StartedAt
                }
            )
            .AsNoTracking()
            .ToListAsync();

        var companyMemberships =
            await (
                from membership
                    in _context.CompanyMembers

                join company
                    in _context.Companies
                    on membership.CompanyId
                    equals company.Id

                where membership.UserId == userId

                orderby membership.JoinedAt
                    descending

                select new
                {
                    membership.Id,
                    membership.CompanyId,

                    CompanyName =
                        company.Name,

                    membership.Role,
                    membership.JoinedAt
                }
            )
            .AsNoTracking()
            .ToListAsync();

        var exportCreatedAt =
            DateTime.UtcNow;

        var personalData = new
        {
            ExportInformation = new
            {
                ExportedAtUtc =
                    exportCreatedAt,

                Application =
                    "SkillJob AI",

                FormatVersion = 1
            },

            Profile = new
            {
                user.Id,
                user.FullName,
                user.Email,
                user.Role,
                user.PhoneNumber,
                user.Location,
                user.Headline,
                user.About,
                user.LinkedInUrl,
                user.GithubUrl,
                user.Website,
                user.CreatedAt,

                HasCv =
                    !string.IsNullOrWhiteSpace(
                        user.CvUrl),

                HasProfileImage =
                    !string.IsNullOrWhiteSpace(
                        user.ProfileImageUrl)
            },

            Applications =
                applications,

            Skills =
                skills,

            Enrollments =
                enrollments,

            LessonProgress =
                lessonProgress,

            CareerGoals =
                careerGoals,

            CompanyMemberships =
                companyMemberships
        };

        var jsonOptions =
            new JsonSerializerOptions
            {
                WriteIndented = true,

                PropertyNamingPolicy =
                    JsonNamingPolicy.CamelCase,

                DefaultIgnoreCondition =
                    JsonIgnoreCondition
                        .WhenWritingNull
            };

        await using var memoryStream =
            new MemoryStream();

        using (
            var archive =
                new ZipArchive(
                    memoryStream,
                    ZipArchiveMode.Create,
                    leaveOpen: true)
        )
        {
            var jsonEntry =
                archive.CreateEntry(
                    "personal-data.json",
                    CompressionLevel.Optimal);

            await using (
                var jsonStream =
                    jsonEntry.Open()
            )
            {
                await JsonSerializer
                    .SerializeAsync(
                        jsonStream,
                        personalData,
                        jsonOptions);
            }

            var profileCvPath =
                ResolveStoredCvPath(
                    user.CvUrl);

            AddFileToArchiveIfExists(
                archive,
                profileCvPath,
                "files/profile/cv.pdf"
            );

            var profileImagePath =
                ResolvePublicFilePath(
                    user.ProfileImageUrl);

            if (profileImagePath is not null)
            {
                var extension =
                    Path.GetExtension(
                        profileImagePath);

                if (string.IsNullOrWhiteSpace(
                        extension))
                {
                    extension = ".bin";
                }

                AddFileToArchiveIfExists(
                    archive,
                    profileImagePath,
                    "files/profile/" +
                    $"profile-image{extension}"
                );
            }

            var applicationFiles =
                await _context.Applications
                    .AsNoTracking()
                    .Where(application =>
                        application.UserId == userId)
                    .Select(application => new
                    {
                        application.Id,
                        application.CvFileUrl,
                        application.CertificateFileUrl,
                        application.PortfolioFileUrl
                    })
                    .ToListAsync();

            foreach (
                var application
                in applicationFiles
            )
            {
                var applicationFolder =
                    "files/applications/" +
                    application.Id;

                AddFileToArchiveIfExists(
                    archive,
                    ResolveApplicationFilePath(
                        application.CvFileUrl),
                    $"{applicationFolder}/cv.pdf"
                );

                AddFileToArchiveIfExists(
                    archive,
                    ResolveApplicationFilePath(
                        application.CertificateFileUrl),
                    $"{applicationFolder}/certificate.pdf"
                );

                AddFileToArchiveIfExists(
                    archive,
                    ResolveApplicationFilePath(
                        application.PortfolioFileUrl),
                    $"{applicationFolder}/portfolio.pdf"
                );
            }
        }

        var fileName =
            "skilljobai-data-export-" +
            $"{exportCreatedAt:yyyy-MM-dd-HHmmss}.zip";

        return (
            true,
            null,
            memoryStream.ToArray(),
            fileName
        );
    }

    public async Task<(
        bool Success,
        string? ErrorMessage
    )> DeleteAccountAsync(
        int userId,
        DeleteAccountRequest request
    )
    {
        if (!request.ConfirmDeletion)
        {
            return (
                false,
                "Account deletion was not confirmed."
            );
        }

        if (string.IsNullOrWhiteSpace(
                request.Password))
        {
            return (
                false,
                "Password is required."
            );
        }

        var user =
            await _context.Users
                .FirstOrDefaultAsync(
                    existingUser =>
                        existingUser.Id == userId);

        if (user is null)
        {
            return (
                false,
                "User not found."
            );
        }

        if (string.Equals(
                user.Role,
                AppRoles.Admin,
                StringComparison.OrdinalIgnoreCase))
        {
            return (
                false,
                "Administrator accounts cannot be deleted through the profile page."
            );
        }

        var passwordIsValid =
            _passwordService.VerifyPassword(
                request.Password,
                user.PasswordHash);

        if (!passwordIsValid)
        {
            return (
                false,
                "The entered password is incorrect."
            );
        }

        /*
         * Die Dateipfade werden vor dem Löschen
         * der Datenbankeinträge ermittelt.
         */
        var profileCvPath =
            ResolveStoredCvPath(
                user.CvUrl);

        var profileImagePath =
            ResolvePublicFilePath(
                user.ProfileImageUrl);

        var applications =
            await _context.Applications
                .Where(application =>
                    application.UserId == userId)
                .ToListAsync();

        var applicationFilePaths =
            applications
                .SelectMany(application =>
                    new[]
                    {
                        ResolveApplicationFilePath(
                            application.CvFileUrl),

                        ResolveApplicationFilePath(
                            application.CertificateFileUrl),

                        ResolveApplicationFilePath(
                            application.PortfolioFileUrl)
                    })
                .Where(path =>
                    !string.IsNullOrWhiteSpace(path))
                .Select(path => path!)
                .Distinct(
                    StringComparer.OrdinalIgnoreCase)
                .ToList();

        var lessonProgresses =
            await _context.LessonProgresses
                .Where(progress =>
                    progress.UserId == userId)
                .ToListAsync();

        var enrollments =
            await _context.Enrollments
                .Where(enrollment =>
                    enrollment.UserId == userId)
                .ToListAsync();

        var userSkills =
            await _context.UserSkills
                .Where(userSkill =>
                    userSkill.UserId == userId)
                .ToListAsync();

        var userCareerGoals =
            await _context.UserCareerGoals
                .Where(userCareerGoal =>
                    userCareerGoal.UserId == userId)
                .ToListAsync();

        var companyMemberships =
            await _context.CompanyMembers
                .Where(companyMember =>
                    companyMember.UserId == userId)
                .ToListAsync();

        var passwordResetTokens =
            await _context.PasswordResetTokens
                .Where(token =>
                    token.UserId == userId)
                .ToListAsync();

        var refreshTokens =
            await _context.RefreshTokens
                .Where(token =>
                    token.UserId == userId)
                .ToListAsync();

        await using var transaction =
            await _context.Database
                .BeginTransactionAsync();

        try
        {
            _context.LessonProgresses
                .RemoveRange(
                    lessonProgresses);

            _context.Enrollments
                .RemoveRange(
                    enrollments);

            _context.UserSkills
                .RemoveRange(
                    userSkills);

            _context.UserCareerGoals
                .RemoveRange(
                    userCareerGoals);

            _context.CompanyMembers
                .RemoveRange(
                    companyMemberships);

            _context.PasswordResetTokens
                .RemoveRange(
                    passwordResetTokens);

            _context.RefreshTokens
                .RemoveRange(
                    refreshTokens);

            _context.Applications
                .RemoveRange(
                    applications);

            _context.Users.Remove(user);

            await _context.SaveChangesAsync();

            await transaction.CommitAsync();
        }
        catch (Exception exception)
        {
            await transaction.RollbackAsync();

            _logger.LogError(
                exception,
                "Fehler beim Löschen des Benutzerkontos. UserId: {UserId}",
                userId);

            throw;
        }

        /*
         * Dateien werden erst nach erfolgreichem
         * Datenbank-Commit entfernt.
         */
        DeleteFileIfExistsSafely(
            profileCvPath);

        DeleteFileIfExistsSafely(
            profileImagePath);

        foreach (
            var applicationFilePath
            in applicationFilePaths
        )
        {
            DeleteFileIfExistsSafely(
                applicationFilePath);
        }

        _logger.LogInformation(
            "Benutzerkonto vollständig gelöscht. UserId: {UserId}",
            userId);

        return (
            true,
            null
        );
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

            CreatedAt =
                user.CreatedAt
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
        string? storedPath
    )
    {
        if (string.IsNullOrWhiteSpace(
                storedPath))
        {
            return null;
        }

        /*
         * Neue CVs:
         * profile-cv/datei.pdf
         *
         * Alte CVs:
         * /uploads/cv/datei.pdf
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

    private string? ResolvePublicFilePath(
        string? fileUrl
    )
    {
        if (string.IsNullOrWhiteSpace(
                fileUrl))
        {
            return null;
        }

        return ResolveSafePath(
            GetWebRootPath(),
            fileUrl);
    }

    private string? ResolveApplicationFilePath(
        string? storedPath
    )
    {
        if (string.IsNullOrWhiteSpace(
                storedPath))
        {
            return null;
        }

        var normalizedStoredPath =
            storedPath
                .TrimStart('/', '\\')
                .Replace(
                    '\\',
                    '/');

        /*
         * Neue Bewerbungsdateien:
         *
         * private_uploads/applications/...
         */
        if (normalizedStoredPath.StartsWith(
                "private_uploads/",
                StringComparison.OrdinalIgnoreCase))
        {
            return ResolveSafePath(
                _environment.ContentRootPath,
                normalizedStoredPath);
        }

        /*
         * Unterstützung für ältere Dateien:
         *
         * uploads/...
         */
        if (normalizedStoredPath.StartsWith(
                "uploads/",
                StringComparison.OrdinalIgnoreCase))
        {
            return ResolveSafePath(
                GetWebRootPath(),
                normalizedStoredPath);
        }

        return null;
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
            ResolvePublicFilePath(
                fileUrl);

        if (filePath is null)
        {
            return;
        }

        DeleteFileIfExists(
            filePath);
    }

    private static void AddFileToArchiveIfExists(
        ZipArchive archive,
        string? sourceFilePath,
        string archiveEntryName
    )
    {
        if (string.IsNullOrWhiteSpace(
                sourceFilePath) ||
            !File.Exists(sourceFilePath))
        {
            return;
        }

        archive.CreateEntryFromFile(
            sourceFilePath,
            archiveEntryName,
            CompressionLevel.Optimal);
    }

    private void DeleteFileIfExistsSafely(
        string? filePath
    )
    {
        if (string.IsNullOrWhiteSpace(
                filePath))
        {
            return;
        }

        try
        {
            if (File.Exists(filePath))
            {
                File.Delete(filePath);
            }
        }
        catch (Exception exception)
        {
            /*
             * Die Datenbanklöschung ist zu diesem
             * Zeitpunkt bereits abgeschlossen.
             */
            _logger.LogError(
                exception,
                "Datei konnte nach der Kontolöschung nicht entfernt werden: {FilePath}",
                filePath);
        }
    }

    private static string? ResolveSafePath(
        string allowedRoot,
        string relativePath
    )
    {
        var normalizedRoot =
            Path.GetFullPath(
                allowedRoot);

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