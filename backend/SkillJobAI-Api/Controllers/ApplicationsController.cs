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

    public ApplicationsController(AppDbContext context, IEmailService emailService)
    {
        _context = context;
        _emailService = emailService;
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

    private async Task<string?> SavePdfFile(IFormFile? file, string folderName, int userId)
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

        using var stream = new FileStream(filePath, FileMode.Create);
        await file.CopyToAsync(stream);

        return $"/uploads/applications/{folderName}/{fileName}";
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
                CvFileUrl = await SavePdfFile(request.CvFile, "cv", userId.Value),
                CertificateFileUrl = await SavePdfFile(request.CertificateFile, "certificates", userId.Value),
                PortfolioFileUrl = await SavePdfFile(request.PortfolioFile, "portfolio", userId.Value)
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
                        company = j.Company != null ? j.Company.Name : null,
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

        var jobSkills = await _context.JobSkills
            .Where(js => js.JobId == job.Id)
            .Include(js => js.Skill)
            .Select(js => js.Skill.Name)
            .ToListAsync();

        var userSkills = await _context.UserSkills
            .Where(us => us.UserId == application.UserId)
            .Include(us => us.Skill)
            .Select(us => us.Skill.Name)
            .ToListAsync();

        var matchedSkills = jobSkills
            .Intersect(userSkills)
            .ToList();

        var missingSkills = jobSkills
            .Except(userSkills)
            .ToList();

        var matchPercentage = jobSkills.Count == 0
            ? 0
            : (int)Math.Round(((double)matchedSkills.Count / jobSkills.Count) * 100);

        var recommendedCourses = await _context.CourseSkills
            .Where(cs => missingSkills.Contains(cs.Skill.Name))
            .Include(cs => cs.Course)
            .Include(cs => cs.Skill)
            .Select(cs => new
            {
                id = cs.Course.Id,
                title = cs.Course.Title,
                skill = cs.Skill.Name
            })
            .Distinct()
            .ToListAsync();

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
            matchPercentage,
            jobSkills,
            userSkills,
            matchedSkills,
            missingSkills,
            recommendedCourses
        });
    }

    [Authorize(Roles = "Recruiter,Admin")]
    [HttpGet("job/{jobId}")]
    public async Task<IActionResult> GetApplicationsForJob(int jobId)
    {
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

        var jobSkills = await _context.JobSkills
            .Where(js => js.JobId == jobId)
            .Include(js => js.Skill)
            .Select(js => js.Skill.Name)
            .ToListAsync();

        var applications = await _context.Applications
            .Where(a => a.JobId == jobId)
            .ToListAsync();

        var result = new List<object>();

        foreach (var application in applications)
        {
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

            var userSkills = await _context.UserSkills
                .Where(us => us.UserId == application.UserId)
                .Include(us => us.Skill)
                .Select(us => us.Skill.Name)
                .ToListAsync();

            var matchedSkills = jobSkills.Intersect(userSkills).ToList();
            var missingSkills = jobSkills.Except(userSkills).ToList();

            var matchPercentage = jobSkills.Count == 0
                ? 0
                : (int)Math.Round(((double)matchedSkills.Count / jobSkills.Count) * 100);

            var recommendedCourses = await _context.CourseSkills
                .Where(cs => missingSkills.Contains(cs.Skill.Name))
                .Include(cs => cs.Course)
                .Include(cs => cs.Skill)
                .Select(cs => new
                {
                    id = cs.Course.Id,
                    title = cs.Course.Title,
                    skill = cs.Skill.Name
                })
                .Distinct()
                .ToListAsync();

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
                candidate,
                matchPercentage,
                jobSkills,
                userSkills,
                matchedSkills,
                missingSkills,
                recommendedCourses
            });
        }

        return Ok(result.OrderByDescending(a => ((dynamic)a).matchPercentage).ToList());
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