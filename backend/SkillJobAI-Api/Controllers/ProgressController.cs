using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SkillJobAI.Api.Models;
using SkillJobAI.Api.Services;

namespace SkillJobAI.Api.Controllers;

[ApiController]
[Route("api/progress")]
public class ProgressController : ControllerBase
{
    private readonly IProgressService _progressService;

    public ProgressController(IProgressService progressService)
    {
        _progressService = progressService;
    }

    private int? GetCurrentUserId()
    {
        var userIdValue = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        return int.TryParse(userIdValue, out var userId) ? userId : null;
    }

    [Authorize]
    [HttpPost("complete")]
    public async Task<IActionResult> CompleteLesson([FromBody] LessonProgressRequest request)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        var userId = GetCurrentUserId();

        if (userId == null)
            return Unauthorized();

        var result = await _progressService.CompleteLessonAsync(userId.Value, request);

        if (!result.Success)
            return BadRequest(new { message = result.ErrorMessage });

        return Ok(result.Progress);
    }

    [Authorize]
    [HttpGet("my")]
    public async Task<IActionResult> MyProgress()
    {
        var userId = GetCurrentUserId();

        if (userId == null)
            return Unauthorized();

        var progress = await _progressService.GetMyProgressAsync(userId.Value);

        return Ok(progress);
    }
}