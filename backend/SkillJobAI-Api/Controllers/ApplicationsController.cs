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

        try
        {
            var application = await _applicationService.CreateApplicationAsync(
                userId.Value,
                request
            );

            if (application == null)
                return BadRequest(new { message = "Job not found." });

            return Ok(application);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
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

        var applications = await _applicationService.GetMyApplicationsAsync(userId.Value);

        return Ok(applications);
    }

    [Authorize(Roles = "Recruiter,Admin")]
    [HttpGet("{id}")]
    public async Task<IActionResult> GetApplication(int id)
    {
        var companyId = await _applicationService.GetApplicationCompanyIdAsync(id);

        if (companyId == null)
            return NotFound(new { message = "Application not found." });

        var canManageCompany = await UserCanManageCompany(companyId.Value);

        if (!canManageCompany)
            return Forbid();

        var application = await _applicationService.GetApplicationByIdAsync(id);

        if (application == null)
            return NotFound(new { message = "Application not found." });

        return Ok(application);
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
        var companyId = await _applicationService.GetJobCompanyIdAsync(jobId);

        if (companyId == null)
            return NotFound(new { message = "Job not found." });

        var canManageCompany = await UserCanManageCompany(companyId.Value);

        if (!canManageCompany)
            return Forbid();

        var response = await _applicationService.GetApplicationsForJobAsync(
            jobId,
            page,
            pageSize,
            search,
            status
        );

        return Ok(response);
    }

    [Authorize(Roles = "Recruiter,Admin")]
    [HttpPut("{id}/status")]
    public async Task<IActionResult> UpdateApplicationStatus(
        int id,
        [FromBody] UpdateApplicationStatusRequest request)
    {
        var companyId = await _applicationService.GetApplicationCompanyIdAsync(id);

        if (companyId == null)
            return NotFound(new { message = "Application not found." });

        var canManageCompany = await UserCanManageCompany(companyId.Value);

        if (!canManageCompany)
            return Forbid();

        var application = await _applicationService.UpdateApplicationStatusAsync(
            id,
            request.Status
        );

        if (application == null)
            return NotFound(new { message = "Application not found." });

        return Ok(application);
    }
}