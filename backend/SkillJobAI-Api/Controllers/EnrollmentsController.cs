using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SkillJobAI.Api.Models;
using SkillJobAI.Api.Services;

namespace SkillJobAI.Api.Controllers;

[ApiController]
[Route("api/enrollments")]
public class EnrollmentsController : ControllerBase
{
    private readonly IEnrollmentService _enrollmentService;

    public EnrollmentsController(IEnrollmentService enrollmentService)
    {
        _enrollmentService = enrollmentService;
    }

    private int? GetCurrentUserId()
    {
        var userIdValue = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        return int.TryParse(userIdValue, out var userId) ? userId : null;
    }

    [Authorize]
    [HttpPost]
    public async Task<IActionResult> Enroll([FromBody] EnrollmentRequest request)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        var userId = GetCurrentUserId();

        if (userId == null)
            return Unauthorized();

        var result = await _enrollmentService.EnrollAsync(userId.Value, request);

        if (!result.Success)
            return BadRequest(new { message = result.ErrorMessage });

        return Ok(result.Enrollment);
    }

    [Authorize]
    [HttpGet("my")]
    public async Task<IActionResult> MyEnrollments()
    {
        var userId = GetCurrentUserId();

        if (userId == null)
            return Unauthorized();

        var enrollments = await _enrollmentService.GetMyEnrollmentsAsync(userId.Value);

        return Ok(enrollments);
    }
}