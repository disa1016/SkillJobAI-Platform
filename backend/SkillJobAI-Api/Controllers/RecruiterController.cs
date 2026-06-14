using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SkillJobAI.Api.Data;

namespace SkillJobAI.Api.Controllers;

[ApiController]
[Route("api/recruiter")]
public class RecruiterController : ControllerBase
{
    private readonly AppDbContext _context;

    public RecruiterController(AppDbContext context)
    {
        _context = context;
    }

    [Authorize(Roles = "Recruiter,Admin")]
    [HttpGet("dashboard")]
    public async Task<IActionResult> GetDashboard()
    {
        var totalCompanies = await _context.Companies.CountAsync();
        var totalJobs = await _context.Jobs.CountAsync();
        var totalApplications = await _context.Applications.CountAsync();

        var pendingApplications = await _context.Applications
            .CountAsync(a => a.Status == "Pending");

        var reviewedApplications = await _context.Applications
            .CountAsync(a => a.Status == "Reviewed");

        var acceptedApplications = await _context.Applications
            .CountAsync(a => a.Status == "Accepted");

        var rejectedApplications = await _context.Applications
            .CountAsync(a => a.Status == "Rejected");

        var recentApplications = await _context.Applications
            .OrderByDescending(a => a.CreatedAt)
            .Take(5)
            .Select(a => new
            {
                id = a.Id,
                status = a.Status,
                createdAt = a.CreatedAt,
                coverLetter = a.CoverLetter,
                job = _context.Jobs
                    .Where(j => j.Id == a.JobId)
                    .Select(j => new
                    {
                        id = j.Id,
                        title = j.Title,
                        company = j.Company != null ? j.Company.Name : null
                    })
                    .FirstOrDefault(),
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

        var topJobsByApplications = await _context.Applications
            .GroupBy(a => a.JobId)
            .Select(g => new
            {
                jobId = g.Key,
                applicationsCount = g.Count(),
                job = _context.Jobs
                    .Where(j => j.Id == g.Key)
                    .Select(j => new
                    {
                        id = j.Id,
                        title = j.Title,
                        company = j.Company != null ? j.Company.Name : null
                    })
                    .FirstOrDefault()
            })
            .OrderByDescending(x => x.applicationsCount)
            .Take(5)
            .ToListAsync();

        return Ok(new
        {
            totalCompanies,
            totalJobs,
            totalApplications,
            pendingApplications,
            reviewedApplications,
            acceptedApplications,
            rejectedApplications,
            recentApplications,
            topJobsByApplications
        });
    }
}