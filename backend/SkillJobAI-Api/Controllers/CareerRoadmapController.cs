using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SkillJobAI.Api.Models.Responses;
using SkillJobAI.Api.Services;

namespace SkillJobAI.Api.Controllers;

[ApiController]
public class CareerRoadmapController : ControllerBase
{
    private readonly ICareerRoadmapService _careerRoadmapService;

    public CareerRoadmapController(ICareerRoadmapService careerRoadmapService)
    {
        _careerRoadmapService = careerRoadmapService;
    }

    private int? GetCurrentUserId()
    {
        var userIdValue = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        return int.TryParse(userIdValue, out var userId) ? userId : null;
    }

    [Authorize]
    [HttpGet("api/career-goals")]
    public async Task<IActionResult> GetCareerGoals()
    {
        var goals = await _careerRoadmapService.GetCareerGoalsAsync();
        return Ok(goals);
    }

    [Authorize]
    [HttpPost("api/career-goals/select/{goalId}")]
    public async Task<IActionResult> SelectCareerGoal(int goalId)
    {
        var userId = GetCurrentUserId();

        if (userId == null)
            return Unauthorized();

        var result = await _careerRoadmapService.SelectCareerGoalAsync(userId.Value, goalId);

        if (result == null)
        {
            return NotFound(new MessageResponse
            {
                Message = "Career goal not found."
            });
        }

        return Ok(result);
    }

    [Authorize]
    [HttpGet("api/career-roadmap/my")]
    public async Task<IActionResult> GetMyCareerRoadmap()
    {
        var userId = GetCurrentUserId();

        if (userId == null)
            return Unauthorized();

        var roadmap = await _careerRoadmapService.GetMyCareerRoadmapAsync(userId.Value);

        return Ok(roadmap);
    }
}