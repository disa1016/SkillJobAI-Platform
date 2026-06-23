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

    [Authorize(Roles = "Recruiter,Admin")]
    [HttpGet("{id}")]
    public async Task<IActionResult> GetCandidate(int id)
    {
        var candidate = await _context.Users
            .Where(u =>
                u.Id == id &&
                (u.Role == "Candidate" || u.Role == "Student"))
            .Select(u => new
            {
                id = u.Id,
                fullName = u.FullName,
                email = u.Email,
                cvUrl = u.CvUrl,
                createdAt = u.CreatedAt
            })
            .FirstOrDefaultAsync();

        if (candidate == null)
            return NotFound(new { message = "Candidate not found." });

        var skills = await _context.UserSkills
            .Where(us => us.UserId == id)
            .Include(us => us.Skill)
            .Select(us => us.Skill.Name)
            .ToListAsync();

        var applications = await _context.Applications
            .Where(a => a.UserId == id)
            .OrderByDescending(a => a.CreatedAt)
            .Select(a => new
            {
                id = a.Id,
                jobId = a.JobId,
                coverLetter = a.CoverLetter,
                status = a.Status,
                cvFileUrl = a.CvFileUrl,
                certificateFileUrl = a.CertificateFileUrl,
                portfolioFileUrl = a.PortfolioFileUrl,
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

        return Ok(new
        {
            candidate.id,
            candidate.fullName,
            candidate.email,
            candidate.cvUrl,
            candidate.createdAt,
            skills,
            skillsCount = skills.Count,
            applicationsCount = applications.Count,
            acceptedApplications = applications.Count(a => a.status == "Accepted"),
            rejectedApplications = applications.Count(a => a.status == "Rejected"),
            applications
        });
    }
}