using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SkillJobAI.Api.Data;

namespace SkillJobAI.Api.Controllers;

[ApiController]
[Route("api/candidate")]
public class CandidateDashboardController : ControllerBase
{
    private readonly AppDbContext _context;

    public CandidateDashboardController(AppDbContext context)
    {
        _context = context;
    }

    [Authorize]
    [HttpGet("dashboard")]
    public async Task<IActionResult> GetDashboard()
    {
        var userIdValue = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

        if (userIdValue == null)
            return Unauthorized();

        var userId = int.Parse(userIdValue);

        var applicationsCount = await _context.Applications
            .CountAsync(a => a.UserId == userId);

        var enrollmentsCount = await _context.Enrollments
            .CountAsync(e => e.UserId == userId);

        var completedLessonsCount = await _context.LessonProgresses
            .CountAsync(p => p.UserId == userId);

        var userSkills = await _context.UserSkills
            .Where(us => us.UserId == userId)
            .Include(us => us.Skill)
            .Select(us => us.Skill.Name)
            .ToListAsync();

        var allJobSkills = await _context.JobSkills
            .Include(js => js.Skill)
            .Select(js => js.Skill.Name)
            .ToListAsync();

        var missingSkills = allJobSkills
            .Except(userSkills)
            .Distinct()
            .Take(5)
            .ToList();

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
            .Take(5)
            .ToListAsync();

        var jobs = await _context.Jobs
            .Include(j => j.Company)
            .ToListAsync();

        var topJobMatches = new List<object>();

        foreach (var job in jobs)
        {
            var jobSkills = await _context.JobSkills
                .Where(js => js.JobId == job.Id)
                .Include(js => js.Skill)
                .Select(js => js.Skill.Name)
                .ToListAsync();

            if (jobSkills.Count == 0)
                continue;

            var matchedSkills = jobSkills
                .Intersect(userSkills)
                .ToList();

            var missingJobSkills = jobSkills
                .Except(userSkills)
                .ToList();

            var matchPercentage = (int)Math.Round(
                ((double)matchedSkills.Count / jobSkills.Count) * 100
            );

            topJobMatches.Add(new
            {
                id = job.Id,
                title = job.Title,
                location = job.Location,
                salary = job.Salary,
                company = job.Company == null ? null : new
                {
                    id = job.Company.Id,
                    name = job.Company.Name
                },
                matchPercentage,
                matchedSkills,
                missingSkills = missingJobSkills
            });
        }

        var orderedTopJobMatches = topJobMatches
            .OrderByDescending(j => ((dynamic)j).matchPercentage)
            .Take(5)
            .ToList();

        return Ok(new
        {
            applicationsCount,
            enrollmentsCount,
            completedLessonsCount,
            userSkills,
            missingSkills,
            recommendedCourses,
            topJobMatches = orderedTopJobMatches
        });
    }
}