using System.Security.Claims;
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

    private int? GetCurrentUserId()
    {
        var userIdValue = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        return int.TryParse(userIdValue, out var userId) ? userId : null;
    }

    private bool IsAdmin()
    {
        return User.IsInRole("Admin");
    }

    [Authorize(Roles = "Recruiter,Admin")]
    [HttpGet("dashboard")]
    public async Task<IActionResult> GetDashboard()
    {
        var userId = GetCurrentUserId();

        if (userId == null)
            return Unauthorized();

        var companyIdsQuery = _context.CompanyMembers
            .Where(cm => cm.UserId == userId.Value)
            .Select(cm => cm.CompanyId);

        if (IsAdmin())
        {
            companyIdsQuery = _context.Companies.Select(c => c.Id);
        }

        var companyIds = await companyIdsQuery.ToListAsync();

        var jobsQuery = _context.Jobs
            .Where(j => j.CompanyId != null && companyIds.Contains(j.CompanyId.Value));

        var jobIds = await jobsQuery
            .Select(j => j.Id)
            .ToListAsync();

        var applicationsQuery = _context.Applications
            .Where(a => jobIds.Contains(a.JobId));

        var totalCompanies = companyIds.Count;
        var totalJobs = await jobsQuery.CountAsync();
        var totalApplications = await applicationsQuery.CountAsync();

        var pendingApplications = await applicationsQuery
            .CountAsync(a => a.Status == "Pending");

        var reviewedApplications = await applicationsQuery
            .CountAsync(a => a.Status == "Reviewed");

        var acceptedApplications = await applicationsQuery
            .CountAsync(a => a.Status == "Accepted");

        var rejectedApplications = await applicationsQuery
            .CountAsync(a => a.Status == "Rejected");

        var recentApplications = await applicationsQuery
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

        var topJobsByApplications = await applicationsQuery
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