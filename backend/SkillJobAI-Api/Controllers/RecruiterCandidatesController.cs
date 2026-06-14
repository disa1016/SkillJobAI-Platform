using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SkillJobAI.Api.Data;

namespace SkillJobAI.Api.Controllers;

[ApiController]
[Route("api/recruiter/candidates")]
public class RecruiterCandidatesController : ControllerBase
{
    private readonly AppDbContext _context;

    public RecruiterCandidatesController(AppDbContext context)
    {
        _context = context;
    }

    [Authorize(Roles = "Recruiter,Admin")]
    [HttpGet]
    public async Task<IActionResult> GetCandidates([FromQuery] string? skill)
    {
        var candidates = await _context.Users
            .Where(u => u.Role == "Candidate" || u.Role == "Student")
            .Select(u => new
            {
                id = u.Id,
                fullName = u.FullName,
                email = u.Email,
                createdAt = u.CreatedAt
            })
            .ToListAsync();

        var result = new List<object>();

        foreach (var candidate in candidates)
        {
            var skills = await _context.UserSkills
                .Where(us => us.UserId == candidate.id)
                .Include(us => us.Skill)
                .Select(us => us.Skill.Name)
                .ToListAsync();

            if (!string.IsNullOrWhiteSpace(skill))
            {
                var hasSkill = skills.Any(s =>
                    s.ToLower().Contains(skill.ToLower()));

                if (!hasSkill)
                    continue;
            }

            var applicationsCount = await _context.Applications
                .CountAsync(a => a.UserId == candidate.id);

            var acceptedApplications = await _context.Applications
                .CountAsync(a => a.UserId == candidate.id && a.Status == "Accepted");

            var rejectedApplications = await _context.Applications
                .CountAsync(a => a.UserId == candidate.id && a.Status == "Rejected");

            result.Add(new
            {
                candidate.id,
                candidate.fullName,
                candidate.email,
                candidate.createdAt,
                skills,
                skillsCount = skills.Count,
                applicationsCount,
                acceptedApplications,
                rejectedApplications
            });
        }

        var orderedResult = result
            .OrderByDescending(c => ((dynamic)c).skillsCount)
            .ToList();

        return Ok(orderedResult);
    }
}