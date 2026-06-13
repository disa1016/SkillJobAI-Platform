using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SkillJobAI.Api.Data;
using SkillJobAI.Api.Models;

namespace SkillJobAI.Api.Controllers;

[ApiController]
[Route("api/admin")]
[Authorize(Roles = "Admin")]
public class AdminController : ControllerBase
{
    private readonly AppDbContext _context;

    public AdminController(AppDbContext context)
    {
        _context = context;
    }

    [HttpGet("dashboard")]
    public async Task<IActionResult> GetDashboard()
    {
        return Ok(new
        {
            totalUsers = await _context.Users.CountAsync(),
            totalCompanies = await _context.Companies.CountAsync(),
            totalJobs = await _context.Jobs.CountAsync(),
            totalApplications = await _context.Applications.CountAsync(),
            totalCourses = await _context.Courses.CountAsync(),
            totalSkills = await _context.Skills.CountAsync()
        });
    }
    [HttpGet("users")]
    public async Task<IActionResult> GetUsers()
    {
        var users = await _context.Users
            .Select(u => new
            {
                id = u.Id,
                fullName = u.FullName,
                email = u.Email,
                role = u.Role,
                createdAt = u.CreatedAt
            })
            .OrderBy(u => u.id)
            .ToListAsync();

        return Ok(users);
    }



    [HttpPut("users/{id}/role")]
    public async Task<IActionResult> UpdateUserRole(int id, [FromBody] UpdateUserRoleRequest request)
    {
        var user = await _context.Users.FindAsync(id);

        if (user == null)
            return NotFound(new { message = "User not found." });

        var allowedRoles = new[] { "Candidate", "Student", "Recruiter", "Admin" };

        if (!allowedRoles.Contains(request.Role))
            return BadRequest(new { message = "Invalid role." });

        user.Role = request.Role;

        await _context.SaveChangesAsync();

        return Ok(new
        {
            message = "User role updated successfully.",
            id = user.Id,
            fullName = user.FullName,
            email = user.Email,
            role = user.Role
        });
    }
    [HttpDelete("users/{id}")]
    public async Task<IActionResult> DeleteUser(int id)
    {
        var user = await _context.Users.FindAsync(id);
        var currentUserId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;

        if (currentUserId == id.ToString())
        {
            return BadRequest(new
            {
                message = "Du kannst deinen eigenen Account nicht löschen."
            });
        }>

        if (user == null)
            return NotFound(new { message = "User not found." });

        _context.Users.Remove(user);
        await _context.SaveChangesAsync();

        return NoContent();
    }
}