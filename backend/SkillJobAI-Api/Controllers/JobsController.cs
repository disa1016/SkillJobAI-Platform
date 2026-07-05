using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SkillJobAI.Api.Data;
using SkillJobAI.Api.Models;
using SkillJobAI.Api.Services;

namespace SkillJobAI.Api.Controllers;

[ApiController]
[Route("api/jobs")]
public class JobsController : ControllerBase
{
    private readonly AppDbContext _context;
    private readonly IJobService _jobService;

    public JobsController(AppDbContext context, IJobService jobService)
    {
        _context = context;
        _jobService = jobService;
    }

    private int? GetCurrentUserId()
    {
        var userIdValue = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

        if (string.IsNullOrEmpty(userIdValue))
            return null;

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
            .AnyAsync(cm =>
                cm.CompanyId == companyId &&
                cm.UserId == userId.Value);
    }

    [HttpGet]
    public async Task<IActionResult> GetJobs(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 10,
        [FromQuery] string? search = null)
    {
        var response = await _jobService.GetJobsAsync(page, pageSize, search);
        return Ok(response);
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetJob(int id)
    {
        var job = await _jobService.GetJobByIdAsync(id);

        if (job == null)
            return NotFound(new { message = "Job not found." });

        return Ok(job);
    }

    [Authorize(Roles = "Recruiter,Admin")]
    [HttpPost]
    public async Task<IActionResult> CreateJob([FromBody] JobRequest request)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        if (request.CompanyId == null)
            return BadRequest(new { message = "CompanyId is required." });

        var companyExists = await _jobService.CompanyExistsAsync(request.CompanyId.Value);

        if (!companyExists)
            return BadRequest(new { message = "Company not found." });

        var canManageCompany = await UserCanManageCompany(request.CompanyId.Value);

        if (!canManageCompany)
            return Forbid();

        var job = await _jobService.CreateJobAsync(request);

        if (job == null)
            return BadRequest(new { message = "Job could not be created." });

        return Ok(job);
    }

    [Authorize(Roles = "Recruiter,Admin")]
    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateJob(
        int id,
        [FromBody] JobRequest request)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        var currentCompanyId = await _jobService.GetJobCompanyIdAsync(id);

        if (currentCompanyId == null)
            return NotFound(new { message = "Job not found." });

        var canManageCurrentCompany = await UserCanManageCompany(currentCompanyId.Value);

        if (!canManageCurrentCompany)
            return Forbid();

        if (request.CompanyId == null)
            return BadRequest(new { message = "CompanyId is required." });

        var companyExists = await _jobService.CompanyExistsAsync(request.CompanyId.Value);

        if (!companyExists)
            return BadRequest(new { message = "Company not found." });

        var canManageNewCompany = await UserCanManageCompany(request.CompanyId.Value);

        if (!canManageNewCompany)
            return Forbid();

        var job = await _jobService.UpdateJobAsync(id, request);

        if (job == null)
            return BadRequest(new { message = "Job could not be updated." });

        return Ok(job);
    }

    [Authorize(Roles = "Recruiter,Admin")]
    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteJob(int id)
    {
        var companyId = await _jobService.GetJobCompanyIdAsync(id);

        if (companyId == null)
            return NotFound(new { message = "Job not found." });

        var canManageCompany = await UserCanManageCompany(companyId.Value);

        if (!canManageCompany)
            return Forbid();

        var deleted = await _jobService.DeleteJobAsync(id);

        if (!deleted)
            return NotFound(new { message = "Job not found." });

        return NoContent();
    }
}