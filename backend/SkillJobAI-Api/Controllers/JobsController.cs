using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SkillJobAI.Api.Data;
using SkillJobAI.Api.Entities;

namespace SkillJobAI.Api.Controllers;

[ApiController]
[Route("api/jobs")]
public class JobsController : ControllerBase
{
    private readonly AppDbContext _context;

    public JobsController(AppDbContext context)
    {
        _context = context;
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
    public async Task<IActionResult> GetJobs()
    {
        var jobs = await _context.Jobs
            .Include(j => j.Company)
            .Select(j => new
            {
                id = j.Id,
                title = j.Title,
                description = j.Description,
                location = j.Location,
                salary = j.Salary,
                createdAt = j.CreatedAt,
                companyId = j.CompanyId,
                companyName = j.Company != null ? j.Company.Name : null,
                company = j.Company == null ? null : new
                {
                    id = j.Company.Id,
                    name = j.Company.Name,
                    location = j.Company.Location
                }
            })
            .ToListAsync();

        return Ok(jobs);
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetJob(int id)
    {
        var job = await _context.Jobs
            .Include(j => j.Company)
            .Where(j => j.Id == id)
            .Select(j => new
            {
                id = j.Id,
                title = j.Title,
                description = j.Description,
                location = j.Location,
                salary = j.Salary,
                createdAt = j.CreatedAt,
                companyId = j.CompanyId,
                companyName = j.Company != null ? j.Company.Name : null,
                company = j.Company == null ? null : new
                {
                    id = j.Company.Id,
                    name = j.Company.Name,
                    description = j.Company.Description,
                    websiteUrl = j.Company.WebsiteUrl,
                    logoUrl = j.Company.LogoUrl,
                    location = j.Company.Location
                }
            })
            .FirstOrDefaultAsync();

        if (job == null)
            return NotFound(new { message = "Job not found." });

        return Ok(job);
    }

    [Authorize(Roles = "Recruiter,Admin")]
    [HttpPost]
    public async Task<IActionResult> CreateJob(Job job)
    {
        if (job.CompanyId == null)
        {
            return BadRequest(new
            {
                message = "CompanyId is required."
            });
        }

        var companyExists = await _context.Companies
            .AnyAsync(c => c.Id == job.CompanyId.Value);

        if (!companyExists)
        {
            return BadRequest(new
            {
                message = "Company not found."
            });
        }

        var canManageCompany = await UserCanManageCompany(job.CompanyId.Value);

        if (!canManageCompany)
        {
            return Forbid();
        }

        job.CreatedAt = DateTime.UtcNow;

        _context.Jobs.Add(job);
        await _context.SaveChangesAsync();

        return Ok(new
        {
            id = job.Id,
            title = job.Title,
            description = job.Description,
            location = job.Location,
            salary = job.Salary,
            createdAt = job.CreatedAt,
            companyId = job.CompanyId
        });
    }

    [Authorize(Roles = "Recruiter,Admin")]
    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateJob(int id, Job request)
    {
        var job = await _context.Jobs.FindAsync(id);

        if (job == null)
            return NotFound(new { message = "Job not found." });

        if (job.CompanyId == null)
            return BadRequest(new { message = "Job has no company." });

        var canManageCurrentCompany = await UserCanManageCompany(job.CompanyId.Value);

        if (!canManageCurrentCompany)
            return Forbid();

        if (request.CompanyId == null)
            return BadRequest(new { message = "CompanyId is required." });

        var newCompanyExists = await _context.Companies
            .AnyAsync(c => c.Id == request.CompanyId.Value);

        if (!newCompanyExists)
            return BadRequest(new { message = "Company not found." });

        var canManageNewCompany = await UserCanManageCompany(request.CompanyId.Value);

        if (!canManageNewCompany)
            return Forbid();

        job.Title = request.Title;
        job.Description = request.Description;
        job.Location = request.Location;
        job.Salary = request.Salary;
        job.CompanyId = request.CompanyId;

        await _context.SaveChangesAsync();

        return Ok(new
        {
            id = job.Id,
            title = job.Title,
            description = job.Description,
            location = job.Location,
            salary = job.Salary,
            createdAt = job.CreatedAt,
            companyId = job.CompanyId
        });
    }

    [Authorize(Roles = "Recruiter,Admin")]
    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteJob(int id)
    {
        var job = await _context.Jobs.FindAsync(id);

        if (job == null)
            return NotFound(new { message = "Job not found." });

        if (job.CompanyId == null)
            return BadRequest(new { message = "Job has no company." });

        var canManageCompany = await UserCanManageCompany(job.CompanyId.Value);

        if (!canManageCompany)
            return Forbid();

        _context.Jobs.Remove(job);
        await _context.SaveChangesAsync();

        return NoContent();
    }
}