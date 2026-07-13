using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SkillJobAI.Api.Services;

namespace SkillJobAI.Api.Controllers;

[ApiController]
[Route("api/candidate")]
public class CandidateDashboardController : ControllerBase
{
    private readonly ICandidateDashboardService _candidateDashboardService;

    public CandidateDashboardController(ICandidateDashboardService candidateDashboardService)
    {
        _candidateDashboardService = candidateDashboardService;
    }

    private int? GetCurrentUserId()
    {
        var userIdValue = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        return int.TryParse(userIdValue, out var userId) ? userId : null;
    }

    [Authorize]
    [HttpGet("dashboard")]
    public async Task<IActionResult> GetDashboard()
    {
        var userId = GetCurrentUserId();

        if (userId == null)
            return Unauthorized();

        var dashboard = await _candidateDashboardService.GetDashboardAsync(userId.Value);

        return Ok(dashboard);
    }
}