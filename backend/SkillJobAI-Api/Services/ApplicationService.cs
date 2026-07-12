using Microsoft.EntityFrameworkCore;
using SkillJobAI.Api.Data;
using SkillJobAI.Api.Entities;
using SkillJobAI.Api.Models;
using SkillJobAI.Api.Models.Responses;

namespace SkillJobAI.Api.Services;

public class ApplicationService : IApplicationService
{
    private readonly AppDbContext _context;
    private readonly IFileStorageService _fileStorageService;
    private readonly IApplicationMatchingService _applicationMatchingService;
    private readonly IEmailService _emailService;

    public ApplicationService(
        AppDbContext context,
        IFileStorageService fileStorageService,
        IApplicationMatchingService applicationMatchingService,
        IEmailService emailService)
    {
        _context = context;
        _fileStorageService = fileStorageService;
        _applicationMatchingService = applicationMatchingService;
        _emailService = emailService;
    }

    public async Task<ApplicationResponse?> CreateApplicationAsync(
        int userId,
        CreateApplicationRequest request)
    {
        var jobExists = await _context.Jobs.AnyAsync(j => j.Id == request.JobId);

        if (!jobExists)
            return null;

        var activeApplication = await _context.Applications
            .AnyAsync(a =>
                a.UserId == userId &&
                a.JobId == request.JobId &&
                a.Status != "Rejected");

        if (activeApplication)
            throw new InvalidOperationException("Du hast bereits eine aktive Bewerbung für diesen Job.");

        var application = new Application
        {
            UserId = userId,
            JobId = request.JobId,
            CoverLetter = request.CoverLetter,
            Status = "Pending",
            CreatedAt = DateTime.UtcNow,
            CvFileUrl = await _fileStorageService.SavePdfFileAsync(request.CvFile, "cv", userId),
            CertificateFileUrl = await _fileStorageService.SavePdfFileAsync(request.CertificateFile, "certificates", userId),
            PortfolioFileUrl = await _fileStorageService.SavePdfFileAsync(request.PortfolioFile, "portfolio", userId)
        };

        _context.Applications.Add(application);
        await _context.SaveChangesAsync();

        return await GetApplicationByIdAsync(application.Id);
    }

    public async Task<List<ApplicationResponse>> GetMyApplicationsAsync(int userId)
    {
        return await _context.Applications
            .Where(a => a.UserId == userId)
            .Select(a => new ApplicationResponse
            {
                Id = a.Id,
                UserId = a.UserId,
                JobId = a.JobId,
                CoverLetter = a.CoverLetter,
                Status = a.Status,
                CvFileUrl = a.CvFileUrl ?? "",
                CertificateFileUrl = a.CertificateFileUrl ?? "",
                PortfolioFileUrl = a.PortfolioFileUrl ?? "",
                CreatedAt = a.CreatedAt,
                Job = _context.Jobs
                    .Where(j => j.Id == a.JobId)
                    .Select(j => new ApplicationJobResponse
                    {
                        Id = j.Id,
                        Title = j.Title,
                        Company = _context.Companies
                            .Where(c => c.Id == j.CompanyId)
                            .Select(c => c.Name)
                            .FirstOrDefault() ?? "",
                        CompanyId = j.CompanyId,
                        Location = j.Location,
                        Salary = j.Salary
                    })
                    .FirstOrDefault()
            })
            .ToListAsync();
    }

    public async Task<ApplicationResponse?> GetApplicationByIdAsync(int id)
    {
        var application = await _context.Applications
            .FirstOrDefaultAsync(a => a.Id == id);

        if (application == null)
            return null;

        var job = await _context.Jobs
            .Include(j => j.Company)
            .FirstOrDefaultAsync(j => j.Id == application.JobId);

        if (job == null)
            return null;

        var candidate = await _context.Users
            .Where(u => u.Id == application.UserId)
            .Select(u => new ApplicationCandidateResponse
            {
                Id = u.Id,
                FullName = u.FullName,
                Email = u.Email,
                CvUrl = u.CvUrl ?? ""
            })
            .FirstOrDefaultAsync();

        var match = await _applicationMatchingService.GetMatchResultAsync(
            job.Id,
            application.UserId
        );

        return new ApplicationResponse
        {
            Id = application.Id,
            UserId = application.UserId,
            JobId = application.JobId,
            CoverLetter = application.CoverLetter,
            Status = application.Status,
            CvFileUrl = application.CvFileUrl ?? "",
            CertificateFileUrl = application.CertificateFileUrl ?? "",
            PortfolioFileUrl = application.PortfolioFileUrl ?? "",
            CreatedAt = application.CreatedAt,
            Candidate = candidate,
            Job = new ApplicationJobResponse
            {
                Id = job.Id,
                Title = job.Title,
                Company = job.Company?.Name ?? "",
                CompanyId = job.CompanyId,
                Location = job.Location,
                Salary = job.Salary
            },
            MatchPercentage = match.MatchPercentage,
            JobSkills = match.JobSkills,
            UserSkills = match.UserSkills,
            MatchedSkills = match.MatchedSkills,
            MissingSkills = match.MissingSkills,
            RecommendedCourses = match.RecommendedCourses
        };
    }

    public async Task<PagedResponse<ApplicationResponse>> GetApplicationsForJobAsync(
        int jobId,
        int page,
        int pageSize,
        string? search,
        string? status)
    {
        if (page < 1)
            page = 1;

        if (pageSize < 1)
            pageSize = 10;

        if (pageSize > 50)
            pageSize = 50;

        var query =
            from application in _context.Applications
            join user in _context.Users on application.UserId equals user.Id
            where application.JobId == jobId
            select new
            {
                Application = application,
                Candidate = new ApplicationCandidateResponse
                {
                    Id = user.Id,
                    FullName = user.FullName,
                    Email = user.Email,
                    CvUrl = user.CvUrl ?? ""
                }
            };

        if (!string.IsNullOrWhiteSpace(status))
            query = query.Where(x => x.Application.Status == status);

        if (!string.IsNullOrWhiteSpace(search))
        {
            var searchTerm = search.ToLower();

            query = query.Where(x =>
                x.Candidate.FullName.ToLower().Contains(searchTerm) ||
                x.Candidate.Email.ToLower().Contains(searchTerm) ||
                x.Application.CoverLetter.ToLower().Contains(searchTerm));
        }

        var totalItems = await query.CountAsync();

        var applications = await query
            .OrderByDescending(x => x.Application.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        var result = new List<ApplicationResponse>();

        foreach (var item in applications)
        {
            var match = await _applicationMatchingService.GetMatchResultAsync(
                jobId,
                item.Application.UserId
            );

            result.Add(new ApplicationResponse
            {
                Id = item.Application.Id,
                UserId = item.Application.UserId,
                JobId = item.Application.JobId,
                CoverLetter = item.Application.CoverLetter,
                Status = item.Application.Status,
                CvFileUrl = item.Application.CvFileUrl ?? "",
                CertificateFileUrl = item.Application.CertificateFileUrl ?? "",
                PortfolioFileUrl = item.Application.PortfolioFileUrl ?? "",
                CreatedAt = item.Application.CreatedAt,
                Candidate = item.Candidate,
                MatchPercentage = match.MatchPercentage,
                JobSkills = match.JobSkills,
                UserSkills = match.UserSkills,
                MatchedSkills = match.MatchedSkills,
                MissingSkills = match.MissingSkills,
                RecommendedCourses = match.RecommendedCourses
            });
        }

        return new PagedResponse<ApplicationResponse>
        {
            Items = result,
            Page = page,
            PageSize = pageSize,
            TotalItems = totalItems,
            TotalPages = (int)Math.Ceiling(totalItems / (double)pageSize)
        };
    }

    public async Task<ApplicationResponse?> UpdateApplicationStatusAsync(int id, string status)
    {
        var application = await _context.Applications
            .FirstOrDefaultAsync(a => a.Id == id);

        if (application == null)
            return null;

        var job = await _context.Jobs
            .Include(j => j.Company)
            .FirstOrDefaultAsync(j => j.Id == application.JobId);

        if (job == null)
            return null;

        application.Status = status;
        await _context.SaveChangesAsync();

        var user = await _context.Users
            .FirstOrDefaultAsync(u => u.Id == application.UserId);

        if (user != null)
        {
            await _emailService.SendEmailAsync(
                user.Email,
                "Dein Bewerbungsstatus wurde aktualisiert - SkillJob AI",
                $@"
                <h2>Bewerbungsstatus aktualisiert</h2>
                <p>Hallo {user.FullName},</p>
                <p>dein Bewerbungsstatus für die Stelle <strong>{job.Title}</strong> wurde aktualisiert.</p>
                <p><strong>Firma:</strong> {job.Company?.Name ?? "Keine Firma angegeben"}</p>
                <p><strong>Neuer Status:</strong> {application.Status}</p>
                <br/>
                <p>Viele Grüße</p>
                <p>Dein SkillJob AI Team</p>"
            );
        }

        return await GetApplicationByIdAsync(id);
    }

    public async Task<int?> GetApplicationCompanyIdAsync(int applicationId)
    {
        return await _context.Applications
            .Where(a => a.Id == applicationId)
            .Join(
                _context.Jobs,
                a => a.JobId,
                j => j.Id,
                (a, j) => j.CompanyId
            )
            .FirstOrDefaultAsync();
    }

    public async Task<int?> GetJobCompanyIdAsync(int jobId)
    {
        return await _context.Jobs
            .Where(j => j.Id == jobId)
            .Select(j => j.CompanyId)
            .FirstOrDefaultAsync();
    }
    public async Task<ApplicationFileDownloadResponse?> GetApplicationFileAsync(
    int applicationId,
    string fileType)
{
    var application = await _context.Applications
        .AsNoTracking()
        .FirstOrDefaultAsync(a => a.Id == applicationId);

    if (application == null)
    {
        return null;
    }

    var normalizedFileType = fileType
        .Trim()
        .ToLowerInvariant();

    var storedFilePath = normalizedFileType switch
    {
        "cv" => application.CvFileUrl,
        "certificate" => application.CertificateFileUrl,
        "portfolio" => application.PortfolioFileUrl,
        _ => null
    };

    if (string.IsNullOrWhiteSpace(storedFilePath))
    {
        return null;
    }

    var normalizedStoredPath = storedFilePath
        .TrimStart('/')
        .Replace('/', Path.DirectorySeparatorChar)
        .Replace('\\', Path.DirectorySeparatorChar);

    var projectRoot = Path.GetFullPath(
        Directory.GetCurrentDirectory());

    string allowedRoot;
    string physicalFilePath;

    if (normalizedStoredPath.StartsWith(
            $"private_uploads{Path.DirectorySeparatorChar}",
            StringComparison.OrdinalIgnoreCase))
    {
        allowedRoot = Path.GetFullPath(
            Path.Combine(
                projectRoot,
                "private_uploads"));

        physicalFilePath = Path.GetFullPath(
            Path.Combine(
                projectRoot,
                normalizedStoredPath));
    }
    else if (normalizedStoredPath.StartsWith(
                 $"uploads{Path.DirectorySeparatorChar}",
                 StringComparison.OrdinalIgnoreCase))
    {
        // Unterstützung für bestehende Dateien,
        // die früher unter wwwroot gespeichert wurden.
        allowedRoot = Path.GetFullPath(
            Path.Combine(
                projectRoot,
                "wwwroot",
                "uploads"));

        physicalFilePath = Path.GetFullPath(
            Path.Combine(
                projectRoot,
                "wwwroot",
                normalizedStoredPath));
    }
    else
    {
        return null;
    }

    var allowedRootWithSeparator =
        allowedRoot.TrimEnd(Path.DirectorySeparatorChar)
        + Path.DirectorySeparatorChar;

    if (!physicalFilePath.StartsWith(
            allowedRootWithSeparator,
            StringComparison.OrdinalIgnoreCase))
    {
        return null;
    }

    if (!File.Exists(physicalFilePath))
    {
        return null;
    }

    var downloadFileName = normalizedFileType switch
    {
        "cv" => $"cv-application-{applicationId}.pdf",
        "certificate" =>
            $"certificate-application-{applicationId}.pdf",
        "portfolio" =>
            $"portfolio-application-{applicationId}.pdf",
        _ => $"application-file-{applicationId}.pdf"
    };

    return new ApplicationFileDownloadResponse
    {
        FilePath = physicalFilePath,
        DownloadFileName = downloadFileName,
        ContentType = "application/pdf"
    };
}
}