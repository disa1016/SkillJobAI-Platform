using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SkillJobAI.Api.Data;

namespace SkillJobAI.Api.Controllers;

[ApiController]
[Route("api/admin")]
[Authorize(Roles = "Admin")]
public class AdminController : ControllerBase
{
    private readonly AppDbContext _context;

    public AdminController(AppDbContext context)
    {
        _context = context;
    }

    [HttpGet("dashboard")]
    public async Task<IActionResult> GetDashboard()
    {
        return Ok(new
        {
            totalUsers = await _context.Users.CountAsync(),
            totalCompanies = await _context.Companies.CountAsync(),
            totalJobs = await _context.Jobs.CountAsync(),
            totalApplications = await _context.Applications.CountAsync(),
            totalCourses = await _context.Courses.CountAsync(),
            totalSkills = await _context.Skills.CountAsync()
        });
    }
}