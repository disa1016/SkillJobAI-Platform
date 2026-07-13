using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SkillJobAI.Api.Models;
using SkillJobAI.Api.Models.Responses;
using SkillJobAI.Api.Services;
using System.Security.Claims;

namespace SkillJobAI.Api.Controllers;

[ApiController]
[Route("api/admin")]
[Authorize(Roles = "Admin")]
public class AdminController : ControllerBase
{
    private readonly IAdminService _adminService;

    public AdminController(IAdminService adminService)
    {
        _adminService = adminService;
    }

    [HttpGet("dashboard")]
    public async Task<IActionResult> GetDashboard()
    {
        var dashboard = await _adminService.GetDashboardAsync();
        return Ok(dashboard);
    }

    [HttpGet("users")]
    public async Task<IActionResult> GetUsers()
    {
        var users = await _adminService.GetUsersAsync();
        return Ok(users);
    }

    [HttpPut("users/{id}/role")]
    public async Task<IActionResult> UpdateUserRole(int id, [FromBody] UpdateUserRoleRequest request)
    {
        var result = await _adminService.UpdateUserRoleAsync(id, request);

        if (result == null)
            return BadRequest(new MessageResponse { Message = "User not found or invalid role." });

        return Ok(result);
    }

    [HttpDelete("users/{id}")]
    public async Task<IActionResult> DeleteUser(int id)
    {
        var currentUserId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

        if (currentUserId == id.ToString())
        {
            return BadRequest(new MessageResponse
            {
                Message = "Du kannst deinen eigenen Account nicht löschen."
            });
        }

        var deleted = await _adminService.DeleteUserAsync(id);

        if (!deleted)
            return NotFound(new MessageResponse { Message = "User not found." });

        return NoContent();
    }
}