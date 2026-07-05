using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SkillJobAI.Api.Models;
using SkillJobAI.Api.Services;

namespace SkillJobAI.Api.Controllers;

[ApiController]
[Route("api/users")]
public class UsersController : ControllerBase
{
    private readonly IUserService _userService;

    public UsersController(IUserService userService)
    {
        _userService = userService;
    }

    private int? GetCurrentUserId()
    {
        var userIdValue = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        return int.TryParse(userIdValue, out var userId) ? userId : null;
    }

    [Authorize]
    [HttpGet("profile")]
    public async Task<IActionResult> Profile()
    {
        var userId = GetCurrentUserId();

        if (userId == null)
            return Unauthorized();

        var user = await _userService.GetProfileAsync(userId.Value);

        if (user == null)
            return NotFound(new { message = "User not found." });

        return Ok(user);
    }

    [Authorize]
    [HttpPost("cv")]
    public async Task<IActionResult> UploadCv(IFormFile file)
    {
        var userId = GetCurrentUserId();

        if (userId == null)
            return Unauthorized();

        var result = await _userService.UploadCvAsync(userId.Value, file);

        if (!result.Success)
            return BadRequest(new { message = result.ErrorMessage });

        return Ok(new
        {
            message = "CV uploaded successfully.",
            cvUrl = result.CvUrl
        });
    }

    [Authorize]
    [HttpDelete("cv")]
    public async Task<IActionResult> DeleteCv()
    {
        var userId = GetCurrentUserId();

        if (userId == null)
            return Unauthorized();

        var deleted = await _userService.DeleteCvAsync(userId.Value);

        if (!deleted)
            return NotFound(new { message = "User not found." });

        return Ok(new { message = "CV deleted successfully." });
    }

    [Authorize(Roles = "Admin")]
    [HttpPut("{id}/role")]
    public async Task<IActionResult> UpdateUserRole(
        int id,
        [FromBody] UpdateUserRoleRequest request)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        var user = await _userService.UpdateUserRoleAsync(id, request);

        if (user == null)
            return NotFound(new { message = "User not found." });

        return Ok(new
        {
            message = "User role updated successfully.",
            user
        });
    }
}