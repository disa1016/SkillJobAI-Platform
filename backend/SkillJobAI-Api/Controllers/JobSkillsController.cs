using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SkillJobAI.Api.Services;

namespace SkillJobAI.Api.Controllers;

[ApiController]
[Route("api/jobs/{jobId}/skills")]
public class JobSkillsController : ControllerBase
{
    private readonly IJobSkillService _jobSkillService;

    public JobSkillsController(IJobSkillService jobSkillService)
    {
        _jobSkillService = jobSkillService;
    }

    [HttpGet]
    public async Task<IActionResult> GetJobSkills(int jobId)
    {
        var skills = await _jobSkillService.GetJobSkillsAsync(jobId);

        if (skills == null)
            return NotFound(new { message = "Job not found." });

        return Ok(skills);
    }

    [Authorize(Roles = "Recruiter,Admin")]
    [HttpPost("{skillId}")]
    public async Task<IActionResult> AddSkillToJob(int jobId, int skillId)
    {
        var result = await _jobSkillService.AddSkillToJobAsync(jobId, skillId);

        if (!result.Success)
            return BadRequest(new { message = result.ErrorMessage });

        return Ok(new
        {
            message = "Skill added to job successfully.",
            jobId,
            skillId
        });
    }

    [Authorize(Roles = "Recruiter,Admin")]
    [HttpDelete("{skillId}")]
    public async Task<IActionResult> RemoveSkillFromJob(int jobId, int skillId)
    {
        var result = await _jobSkillService.RemoveSkillFromJobAsync(jobId, skillId);

        if (!result.Success)
            return NotFound(new { message = result.ErrorMessage });

        return Ok(new
        {
            message = "Skill removed from job successfully.",
            jobId,
            skillId
        });
    }
}