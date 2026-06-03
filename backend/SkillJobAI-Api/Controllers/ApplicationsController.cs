using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SkillJobAI.Api.Data;
using SkillJobAI.Api.Entities;
using SkillJobAI.Api.Models;

namespace SkillJobAI.Api.Controllers;

[ApiController]
[Route("api/applications")]
public class ApplicationsController : ControllerBase
{
    private readonly AppDbContext _context;

    public ApplicationsController(AppDbContext context)
    {
        _context = context;
    }

    // Bewerbung erstellen
    [Authorize]
    [HttpPost]
    public async Task<IActionResult> CreateApplication(Application application)
    {
        var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

        if (userId == null)
            return Unauthorized();

        var jobExists = await _context.Jobs
            .AnyAsync(j => j.Id == application.JobId);

        if (!jobExists)
        {
            return BadRequest(new
            {
                message = "Job not found."
            });
        }

        application.UserId = int.Parse(userId);
        application.CreatedAt = DateTime.UtcNow;
        application.Status = "Pending";

        _context.Applications.Add(application);
        await _context.SaveChangesAsync();

        return Ok(application);
    }

    // Meine Bewerbungen
    [Authorize]
    [HttpGet("my")]
    public async Task<IActionResult> MyApplications()
    {
        var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

        if (userId == null)
            return Unauthorized();

        var applications = await _context.Applications
            .Where(a => a.UserId == int.Parse(userId))
            .Select(a => new
            {
                id = a.Id,
                jobId = a.JobId,
                coverLetter = a.CoverLetter,
                status = a.Status,
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

    // Bewerbungen für einen bestimmten Job abrufen
    [Authorize]
    [HttpGet("job/{jobId}")]
    public async Task<IActionResult> GetApplicationsForJob(int jobId)
    {
        var jobExists = await _context.Jobs
            .AnyAsync(j => j.Id == jobId);

        if (!jobExists)
        {
            return NotFound(new
            {
                message = "Job not found."
            });
        }

        var applications = await _context.Applications
            .Where(a => a.JobId == jobId)
            .Select(a => new
            {
                id = a.Id,
                userId = a.UserId,
                coverLetter = a.CoverLetter,
                status = a.Status,
                createdAt = a.CreatedAt,
                candidate = _context.Users
                    .Where(u => u.Id == a.UserId)
                    .Select(u => new
                    {
                        id = u.Id,
                        fullName = u.FullName,
                        email = u.Email
                    })
                    .FirstOrDefault()
            })
            .ToListAsync();

        return Ok(applications);
    }

    [Authorize]
    [HttpPut("{id}/status")]
    public async Task<IActionResult> UpdateApplicationStatus(
    int id,
    [FromBody] UpdateApplicationStatusRequest request)
    {
        var application = await _context.Applications
            .FirstOrDefaultAsync(a => a.Id == id);

        if (application == null)
        {
            return NotFound(new
            {
                message = "Application not found."
            });
        }

        application.Status = request.Status;

        await _context.SaveChangesAsync();

        return Ok(application);
    }
}