using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SkillJobAI.Api.Data;

namespace SkillJobAI.Api.Controllers;

[ApiController]
[Route("api/jobs")]
public class SkillGapController : ControllerBase
{
    private readonly AppDbContext _context;

    public SkillGapController(AppDbContext context)
    {
        _context = context;
    }

    [Authorize]
    [HttpGet("{jobId}/skill-gap")]
    public async Task<IActionResult> GetSkillGap(int jobId)
    {
        var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

        if (userId == null)
            return Unauthorized();

        var job = await _context.Jobs
            .FirstOrDefaultAsync(j => j.Id == jobId);

        if (job == null)
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

        var userSkills = await _context.UserSkills
            .Where(us => us.UserId == int.Parse(userId))
            .Include(us => us.Skill)
            .Select(us => us.Skill.Name)
            .ToListAsync();

        var matchedSkills = jobSkills
            .Intersect(userSkills)
            .ToList();

        var missingSkills = jobSkills
            .Except(userSkills)
            .ToList();

        var hasJobSkills = jobSkills.Any();

        var matchPercentage = hasJobSkills
            ? (int)Math.Round(((double)matchedSkills.Count / jobSkills.Count) * 100)
            : 0;

        var recommendedCourses = await _context.CourseSkills
            .Where(cs => missingSkills.Contains(cs.Skill.Name))
            .Include(cs => cs.Course)
            .Select(cs => new
            {
                id = cs.Course.Id,
                title = cs.Course.Title
            })
            .Distinct()
            .ToListAsync();

        return Ok(new
        {
            jobId = job.Id,
            jobTitle = job.Title,
            hasJobSkills,
            matchPercentage,
            jobSkills,
            userSkills,
            matchedSkills,
            missingSkills,
            recommendedCourses
        });
    }
}