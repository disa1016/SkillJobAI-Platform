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

    [Authorize]
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

        return Ok(new
        {
            totalCompanies,
            totalJobs,
            totalApplications,
            pendingApplications,
            reviewedApplications,
            acceptedApplications,
            rejectedApplications
        });
    }
}