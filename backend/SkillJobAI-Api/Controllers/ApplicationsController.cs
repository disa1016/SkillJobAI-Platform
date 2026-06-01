using System.Runtime.InteropServices.Marshalling;
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SkillJobAI.Api.Data;
using SkillJobAI.Api.Entities;

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
                        company = j.Company,
                        location = j.Location,
                        salary = j.Salary
                    })
                    .FirstOrDefault()
            })
            .ToListAsync();

        return Ok(applications);
    }
}