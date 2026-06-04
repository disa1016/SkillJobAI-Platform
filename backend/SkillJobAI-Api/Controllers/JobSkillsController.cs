using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SkillJobAI.Api.Data;
using SkillJobAI.Api.Entities;

namespace SkillJobAI.Api.Controllers;

[ApiController]
[Route("api/jobs/{jobId}/skills")]
public class JobSkillsController : ControllerBase
{
    private readonly AppDbContext _context;

    public JobSkillsController(AppDbContext context)
    {
        _context = context;
    }

    [HttpGet]
    public async Task<IActionResult> GetJobSkills(int jobId)
    {
        var jobExists = await _context.Jobs.AnyAsync(j => j.Id == jobId);

        if (!jobExists)
        {
            return NotFound(new { message = "Job not found." });
        }

        var skills = await _context.JobSkills
            .Where(js => js.JobId == jobId)
            .Include(js => js.Skill)
            .Select(js => new
            {
                id = js.Skill.Id,
                name = js.Skill.Name
            })
            .ToListAsync();

        return Ok(skills);
    }

    [Authorize]
    [HttpPost("{skillId}")]
    public async Task<IActionResult> AddSkillToJob(int jobId, int skillId)
    {
        var jobExists = await _context.Jobs.AnyAsync(j => j.Id == jobId);

        if (!jobExists)
        {
            return NotFound(new { message = "Job not found." });
        }

        var skillExists = await _context.Skills.AnyAsync(s => s.Id == skillId);

        if (!skillExists)
        {
            return NotFound(new { message = "Skill not found." });
        }

        var alreadyExists = await _context.JobSkills
            .AnyAsync(js => js.JobId == jobId && js.SkillId == skillId);

        if (alreadyExists)
        {
            return BadRequest(new { message = "Skill already added to this job." });
        }

        var jobSkill = new JobSkill
        {
            JobId = jobId,
            SkillId = skillId
        };

        _context.JobSkills.Add(jobSkill);
        await _context.SaveChangesAsync();

        return Ok(new
        {
            message = "Skill added to job successfully.",
            jobId,
            skillId
        });
    }

    [Authorize]
    [HttpDelete("{skillId}")]
    public async Task<IActionResult> RemoveSkillFromJob(int jobId, int skillId)
    {
        var jobSkill = await _context.JobSkills
            .FirstOrDefaultAsync(js => js.JobId == jobId && js.SkillId == skillId);

        if (jobSkill == null)
        {
            return NotFound(new { message = "Skill is not assigned to this job." });
        }

        _context.JobSkills.Remove(jobSkill);
        await _context.SaveChangesAsync();

        return Ok(new
        {
            message = "Skill removed from job successfully.",
            jobId,
            skillId
        });
    }
}