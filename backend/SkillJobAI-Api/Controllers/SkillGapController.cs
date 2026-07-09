using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SkillJobAI.Api.Services;

namespace SkillJobAI.Api.Controllers;

[ApiController]
[Route("api/jobs")]
public class SkillGapController : ControllerBase
{
    private readonly ISkillGapService _skillGapService;

    public SkillGapController(ISkillGapService skillGapService)
    {
        _skillGapService = skillGapService;
    }

    private int? GetCurrentUserId()
    {
        var userIdValue = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        return int.TryParse(userIdValue, out var userId) ? userId : null;
    }

    [Authorize]
    [HttpGet("{jobId}/skill-gap")]
    public async Task<IActionResult> GetSkillGap(int jobId)
    {
        var userId = GetCurrentUserId();

        if (userId == null)
            return Unauthorized();

        var result = await _skillGapService.GetSkillGapAsync(userId.Value, jobId);

        if (result == null)
            return NotFound(new { message = "Job not found." });

        return Ok(result);
    }
}