using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SkillJobAI.Api.Data;
using SkillJobAI.Api.Models;
using SkillJobAI.Api.Services;

namespace SkillJobAI.Api.Controllers;

[ApiController]
[Route("api/applications")]
public class ApplicationsController : ControllerBase
{
    private readonly AppDbContext _context;
    private readonly IApplicationService _applicationService;

    public ApplicationsController(
        AppDbContext context,
        IApplicationService applicationService)
    {
        _context = context;
        _applicationService = applicationService;
    }

    private int? GetCurrentUserId()
    {
        var userIdValue =
            User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

        return int.TryParse(userIdValue, out var userId)
            ? userId
            : null;
    }

    private bool IsAdmin()
    {
        return User.IsInRole("Admin");
    }

    private async Task<bool> UserCanManageCompany(
        int companyId)
    {
        if (IsAdmin())
        {
            return true;
        }

        var userId = GetCurrentUserId();

        if (userId == null)
        {
            return false;
        }

        return await _context.CompanyMembers
            .AnyAsync(companyMember =>
                companyMember.CompanyId == companyId &&
                companyMember.UserId == userId.Value);
    }

    [Authorize(Roles = "Candidate")]
    [HttpPost]
    [Consumes("multipart/form-data")]
    public async Task<IActionResult> CreateApplication(
        [FromForm] CreateApplicationRequest request)
    {
        var userId = GetCurrentUserId();

        if (userId == null)
        {
            return Unauthorized();
        }

        try
        {
            var application =
                await _applicationService.CreateApplicationAsync(
                    userId.Value,
                    request);

            if (application == null)
            {
                return BadRequest(new
                {
                    message = "Job not found."
                });
            }

            return Ok(application);
        }
        catch (InvalidOperationException exception)
        {
            return BadRequest(new
            {
                message = exception.Message
            });
        }
        catch (Exception exception)
        {
            return BadRequest(new
            {
                message = exception.Message
            });
        }
    }

    [Authorize(Roles = "Candidate")]
    [HttpGet("my")]
    public async Task<IActionResult> MyApplications()
    {
        var userId = GetCurrentUserId();

        if (userId == null)
        {
            return Unauthorized();
        }

        var applications =
            await _applicationService.GetMyApplicationsAsync(
                userId.Value);

        return Ok(applications);
    }

    [Authorize(Roles = "Recruiter,Admin")]
    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetApplication(
        int id)
    {
        var companyId =
            await _applicationService
                .GetApplicationCompanyIdAsync(id);

        if (companyId == null)
        {
            return NotFound(new
            {
                message = "Application not found."
            });
        }

        var canManageCompany =
            await UserCanManageCompany(companyId.Value);

        if (!canManageCompany)
        {
            return Forbid();
        }

        var application =
            await _applicationService
                .GetApplicationByIdAsync(id);

        if (application == null)
        {
            return NotFound(new
            {
                message = "Application not found."
            });
        }

        return Ok(application);
    }

    [Authorize(Roles = "Candidate,Recruiter,Admin")]
    [HttpGet("{id:int}/files/{fileType}")]
    public async Task<IActionResult> DownloadApplicationFile(
        int id,
        string fileType)
    {
        var userId = GetCurrentUserId();

        if (userId == null)
        {
            return Unauthorized();
        }

        var normalizedFileType = fileType
            .Trim()
            .ToLowerInvariant();

        if (normalizedFileType is not
            ("cv" or "certificate" or "portfolio"))
        {
            return BadRequest(new
            {
                message =
                    "Ungültiger Dateityp. Erlaubt sind: " +
                    "cv, certificate und portfolio."
            });
        }

        var applicationAccess = await (
            from application in _context.Applications
            join job in _context.Jobs
                on application.JobId equals job.Id
            where application.Id == id
            select new
            {
                application.UserId,
                job.CompanyId
            })
            .AsNoTracking()
            .FirstOrDefaultAsync();

        if (applicationAccess == null)
        {
            return NotFound(new
            {
                message = "Application not found."
            });
        }

        var isAllowed = false;

        if (IsAdmin())
        {
            isAllowed = true;
        }
        else if (User.IsInRole("Candidate"))
        {
            isAllowed =
                applicationAccess.UserId == userId.Value;
        }
      else if (User.IsInRole("Recruiter"))
{
    if (applicationAccess.CompanyId == null)
    {
        return NotFound(new
        {
            message = "Company not found."
        });
    }

    isAllowed = await UserCanManageCompany(
        applicationAccess.CompanyId.Value);
}

        if (!isAllowed)
        {
            return Forbid();
        }

        var file =
            await _applicationService.GetApplicationFileAsync(
                id,
                normalizedFileType);

        if (file == null)
        {
            return NotFound(new
            {
                message =
                    "Die angeforderte Datei wurde nicht gefunden."
            });
        }

        return PhysicalFile(
            file.FilePath,
            file.ContentType,
            file.DownloadFileName,
            enableRangeProcessing: true);
    }

    [Authorize(Roles = "Recruiter,Admin")]
    [HttpGet("job/{jobId:int}")]
    public async Task<IActionResult> GetApplicationsForJob(
        int jobId,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 10,
        [FromQuery] string? search = null,
        [FromQuery] string? status = null)
    {
        var companyId =
            await _applicationService
                .GetJobCompanyIdAsync(jobId);

        if (companyId == null)
        {
            return NotFound(new
            {
                message = "Job not found."
            });
        }

        var canManageCompany =
            await UserCanManageCompany(companyId.Value);

        if (!canManageCompany)
        {
            return Forbid();
        }

        var response =
            await _applicationService
                .GetApplicationsForJobAsync(
                    jobId,
                    page,
                    pageSize,
                    search,
                    status);

        return Ok(response);
    }

    [Authorize(Roles = "Recruiter,Admin")]
    [HttpPut("{id:int}/status")]
    public async Task<IActionResult> UpdateApplicationStatus(
        int id,
        [FromBody] UpdateApplicationStatusRequest request)
    {
        var companyId =
            await _applicationService
                .GetApplicationCompanyIdAsync(id);

        if (companyId == null)
        {
            return NotFound(new
            {
                message = "Application not found."
            });
        }

        var canManageCompany =
            await UserCanManageCompany(companyId.Value);

        if (!canManageCompany)
        {
            return Forbid();
        }

        var application =
            await _applicationService
                .UpdateApplicationStatusAsync(
                    id,
                    request.Status);

        if (application == null)
        {
            return NotFound(new
            {
                message = "Application not found."
            });
        }

        return Ok(application);
    }
}