using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SkillJobAI.Api.Services;

namespace SkillJobAI.Api.Controllers;

[ApiController]
[Route("api/users/skills")]
public class UserSkillsController : ControllerBase
{
    private readonly IUserSkillService _userSkillService;

    public UserSkillsController(IUserSkillService userSkillService)
    {
        _userSkillService = userSkillService;
    }

    private int? GetCurrentUserId()
    {
        var userIdValue = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

        return int.TryParse(userIdValue, out var userId)
            ? userId
            : null;
    }

    [Authorize]
    [HttpGet("my")]
    public async Task<IActionResult> GetMySkills()
    {
        var userId = GetCurrentUserId();

        if (userId == null)
            return Unauthorized();

        var skills = await _userSkillService.GetMySkillsAsync(userId.Value);

        return Ok(skills);
    }

    [Authorize]
    [HttpPost("{skillId}")]
    public async Task<IActionResult> AddSkillToMe(int skillId)
    {
        var userId = GetCurrentUserId();

        if (userId == null)
            return Unauthorized();

        var result = await _userSkillService.AddSkillToUserAsync(
            userId.Value,
            skillId);

        if (!result.Success)
            return BadRequest(new { message = result.ErrorMessage });

        return Ok(new
        {
            message = "Skill added to profile successfully."
        });
    }

    [Authorize]
    [HttpDelete("{skillId}")]
    public async Task<IActionResult> RemoveSkillFromMe(int skillId)
    {
        var userId = GetCurrentUserId();

        if (userId == null)
            return Unauthorized();

        var result = await _userSkillService.RemoveSkillFromUserAsync(
            userId.Value,
            skillId);

        if (!result.Success)
            return NotFound(new { message = result.ErrorMessage });

        return Ok(new
        {
            message = "Skill removed from profile successfully."
        });
    }
}