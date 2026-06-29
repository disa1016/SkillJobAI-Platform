using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SkillJobAI.Api.Data;
using SkillJobAI.Api.Entities;
using SkillJobAI.Api.Models;
using SkillJobAI.Api.Services;

namespace SkillJobAI.Api.Controllers;

[ApiController]
[Route("api/applications")]
public class ApplicationsController : ControllerBase
{
    private readonly AppDbContext _context;
    private readonly IEmailService _emailService;
    private readonly IFileStorageService _fileStorageService;
    private readonly IApplicationMatchingService _applicationMatchingService;

    public ApplicationsController(
        AppDbContext context,
        IEmailService emailService,
        IFileStorageService fileStorageService,
        IApplicationMatchingService applicationMatchingService)
    {
        _context = context;
        _emailService = emailService;
        _fileStorageService = fileStorageService;
        _applicationMatchingService = applicationMatchingService;
    }

    private int? GetCurrentUserId()
    {
        var userIdValue = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        return int.TryParse(userIdValue, out var userId) ? userId : null;
    }

    private bool IsAdmin()
    {
        return User.IsInRole("Admin");
    }

    private async Task<bool> UserCanManageCompany(int companyId)
    {
        if (IsAdmin())
            return true;

        var userId = GetCurrentUserId();

        if (userId == null)
            return false;

        return await _context.CompanyMembers
            .AnyAsync(cm => cm.CompanyId == companyId && cm.UserId == userId.Value);
    }

    [Authorize(Roles = "Candidate")]
    [HttpPost]
    [Consumes("multipart/form-data")]
    public async Task<IActionResult> CreateApplication([FromForm] CreateApplicationRequest request)
    {
        var userId = GetCurrentUserId();

        if (userId == null)
            return Unauthorized();

        var jobExists = await _context.Jobs.AnyAsync(j => j.Id == request.JobId);

        if (!jobExists)
            return BadRequest(new { message = "Job not found." });

        var activeApplication = await _context.Applications
            .AnyAsync(a =>
                a.UserId == userId.Value &&
                a.JobId == request.JobId &&
                a.Status != "Rejected");

        if (activeApplication)
        {
            return BadRequest(new
            {
                message = "Du hast bereits eine aktive Bewerbung für diesen Job."
            });
        }

        try
        {
            var application = new Application
            {
                UserId = userId.Value,
                JobId = request.JobId,
                CoverLetter = request.CoverLetter,
                Status = "Pending",
                CreatedAt = DateTime.UtcNow,
                CvFileUrl = await _fileStorageService.SavePdfFileAsync(request.CvFile, "cv", userId.Value),
                CertificateFileUrl = await _fileStorageService.SavePdfFileAsync(request.CertificateFile, "certificates", userId.Value),
                PortfolioFileUrl = await _fileStorageService.SavePdfFileAsync(request.PortfolioFile, "portfolio", userId.Value)
            };

            _context.Applications.Add(application);
            await _context.SaveChangesAsync();

            return Ok(application);
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [Authorize(Roles = "Candidate")]
    [HttpGet("my")]
    public async Task<IActionResult> MyApplications()
    {
        var userId = GetCurrentUserId();

        if (userId == null)
            return Unauthorized();

        var applications = await _context.Applications
            .Where(a => a.UserId == userId.Value)
            .Select(a => new
            {
                id = a.Id,
                jobId = a.JobId,
                coverLetter = a.CoverLetter,
                status = a.Status,
                cvFileUrl = a.CvFileUrl,
                certificateFileUrl = a.CertificateFileUrl,
                portfolioFileUrl = a.PortfolioFileUrl,
                createdAt = a.CreatedAt,
                job = _context.Jobs
                    .Where(j => j.Id == a.JobId)
                    .Select(j => new
                    {
                        id = j.Id,
                        title = j.Title,
                        company = _context.Companies
                            .Where(c => c.Id == j.CompanyId)
                            .Select(c => c.Name)
                            .FirstOrDefault(),
                        location = j.Location,
                        salary = j.Salary
                    })
                    .FirstOrDefault()
            })
            .ToListAsync();

        return Ok(applications);
    }

    [Authorize(Roles = "Recruiter,Admin")]
    [HttpGet("{id}")]
    public async Task<IActionResult> GetApplication(int id)
    {
        var application = await _context.Applications
            .FirstOrDefaultAsync(a => a.Id == id);

        if (application == null)
            return NotFound(new { message = "Application not found." });

        var job = await _context.Jobs
            .Include(j => j.Company)
            .FirstOrDefaultAsync(j => j.Id == application.JobId);

        if (job == null)
            return NotFound(new { message = "Job not found." });

        if (job.CompanyId == null)
            return BadRequest(new { message = "Job has no company." });

        var canManageCompany = await UserCanManageCompany(job.CompanyId.Value);

        if (!canManageCompany)
            return Forbid();

        var candidate = await _context.Users
            .Where(u => u.Id == application.UserId)
            .Select(u => new
            {
                id = u.Id,
                fullName = u.FullName,
                email = u.Email,
                cvUrl = u.CvUrl
            })
            .FirstOrDefaultAsync();

        var match = await _applicationMatchingService.GetMatchResultAsync(
            job.Id,
            application.UserId
        );

        return Ok(new
        {
            id = application.Id,
            userId = application.UserId,
            jobId = application.JobId,
            coverLetter = application.CoverLetter,
            status = application.Status,
            cvFileUrl = application.CvFileUrl,
            certificateFileUrl = application.CertificateFileUrl,
            portfolioFileUrl = application.PortfolioFileUrl,
            createdAt = application.CreatedAt,
            candidate,
            job = new
            {
                id = job.Id,
                title = job.Title,
                company = job.Company != null ? job.Company.Name : null,
                companyId = job.CompanyId,
                location = job.Location,
                salary = job.Salary
            },
            matchPercentage = match.MatchPercentage,
            jobSkills = match.JobSkills,
            userSkills = match.UserSkills,
            matchedSkills = match.MatchedSkills,
            missingSkills = match.MissingSkills,
            recommendedCourses = match.RecommendedCourses
        });
    }

[Authorize(Roles = "Recruiter,Admin")]
[HttpGet("job/{jobId}")]
public async Task<IActionResult> GetApplicationsForJob(
    int jobId,
    [FromQuery] int page = 1,
    [FromQuery] int pageSize = 10,
    [FromQuery] string? search = null,
    [FromQuery] string? status = null)
{
    if (page < 1)
        page = 1;

    if (pageSize < 1)
        pageSize = 10;

    if (pageSize > 50)
        pageSize = 50;

    var job = await _context.Jobs
        .Include(j => j.Company)
        .FirstOrDefaultAsync(j => j.Id == jobId);

    if (job == null)
        return NotFound(new { message = "Job not found." });

    if (job.CompanyId == null)
        return BadRequest(new { message = "Job has no company." });

    var canManageCompany = await UserCanManageCompany(job.CompanyId.Value);

    if (!canManageCompany)
        return Forbid();

    var query =
        from application in _context.Applications
        join user in _context.Users on application.UserId equals user.Id
        where application.JobId == jobId
        select new
        {
            application,
            candidate = new
            {
                id = user.Id,
                fullName = user.FullName,
                email = user.Email,
                cvUrl = user.CvUrl
            }
        };

    if (!string.IsNullOrWhiteSpace(status))
    {
        query = query.Where(x => x.application.Status == status);
    }

    if (!string.IsNullOrWhiteSpace(search))
    {
        var searchTerm = search.ToLower();

        query = query.Where(x =>
            x.candidate.fullName.ToLower().Contains(searchTerm) ||
            x.candidate.email.ToLower().Contains(searchTerm) ||
            x.application.CoverLetter.ToLower().Contains(searchTerm));
    }

    var totalItems = await query.CountAsync();

    var applications = await query
        .OrderByDescending(x => x.application.CreatedAt)
        .Skip((page - 1) * pageSize)
        .Take(pageSize)
        .ToListAsync();

    var result = new List<object>();

    foreach (var item in applications)
    {
        var application = item.application;

        var match = await _applicationMatchingService.GetMatchResultAsync(
            jobId,
            application.UserId
        );

        result.Add(new
        {
            id = application.Id,
            userId = application.UserId,
            coverLetter = application.CoverLetter,
            status = application.Status,
            cvFileUrl = application.CvFileUrl,
            certificateFileUrl = application.CertificateFileUrl,
            portfolioFileUrl = application.PortfolioFileUrl,
            createdAt = application.CreatedAt,
            candidate = item.candidate,
            matchPercentage = match.MatchPercentage,
            jobSkills = match.JobSkills,
            userSkills = match.UserSkills,
            matchedSkills = match.MatchedSkills,
            missingSkills = match.MissingSkills,
            recommendedCourses = match.RecommendedCourses
        });
    }

    var response = new PagedResponse<object>
    {
        Items = result,
        Page = page,
        PageSize = pageSize,
        TotalItems = totalItems,
        TotalPages = (int)Math.Ceiling(totalItems / (double)pageSize)
    };

    return Ok(response);
}

    [Authorize(Roles = "Recruiter,Admin")]
    [HttpPut("{id}/status")]
    public async Task<IActionResult> UpdateApplicationStatus(
        int id,
        [FromBody] UpdateApplicationStatusRequest request)
    {
        var application = await _context.Applications
            .FirstOrDefaultAsync(a => a.Id == id);

        if (application == null)
            return NotFound(new { message = "Application not found." });

        var job = await _context.Jobs
            .Include(j => j.Company)
            .FirstOrDefaultAsync(j => j.Id == application.JobId);

        if (job == null)
            return NotFound(new { message = "Job not found." });

        if (job.CompanyId == null)
            return BadRequest(new { message = "Job has no company." });

        var canManageCompany = await UserCanManageCompany(job.CompanyId.Value);

        if (!canManageCompany)
            return Forbid();

        application.Status = request.Status;

        await _context.SaveChangesAsync();

        var user = await _context.Users
            .FirstOrDefaultAsync(u => u.Id == application.UserId);

        try
        {
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
        }
        catch (Exception ex)
        {
            Console.WriteLine("EMAIL ERROR:");
            Console.WriteLine(ex.Message);
            Console.WriteLine(ex.InnerException?.Message);
        }

        return Ok(application);
    }
}