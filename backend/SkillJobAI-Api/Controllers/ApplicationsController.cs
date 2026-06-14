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
        var userIdValue = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

        if (userIdValue == null)
            return Unauthorized();

        var userId = int.Parse(userIdValue);

        var jobExists = await _context.Jobs
            .AnyAsync(j => j.Id == application.JobId);

        if (!jobExists)
        {
            return BadRequest(new
            {
                message = "Job not found."
            });
        }

        var activeApplication = await _context.Applications
            .AnyAsync(a =>
                a.UserId == userId &&
                a.JobId == application.JobId &&
                a.Status != "Rejected");

        if (activeApplication)
        {
            return BadRequest(new
            {
                message = "Du hast bereits eine aktive Bewerbung für diesen Job."
            });
        }

        application.UserId = userId;
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
                    email = u.Email
                })
                .FirstOrDefaultAsync();

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

            result.Add(new
            {
                id = application.Id,
                userId = application.UserId,
                coverLetter = application.CoverLetter,
                status = application.Status,
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

        var orderedResult = result
            .OrderByDescending(a => ((dynamic)a).matchPercentage)
            .ToList();

        return Ok(orderedResult);
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