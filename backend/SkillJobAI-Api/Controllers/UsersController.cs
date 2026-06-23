using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SkillJobAI.Api.Data;
using SkillJobAI.Api.Constants;
using SkillJobAI.Api.Models;

namespace SkillJobAI.Api.Controllers;

[ApiController]
[Route("api/users")]
public class UsersController : ControllerBase
{
    private readonly AppDbContext _context;

    public UsersController(AppDbContext context)
    {
        _context = context;
    }

    [Authorize]
    [HttpGet("profile")]
    public async Task<IActionResult> Profile()
    {
        var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

        if (userId == null)
            return Unauthorized();

        var user = await _context.Users
            .FirstOrDefaultAsync(u => u.Id == int.Parse(userId));

        if (user == null)
            return NotFound();

        return Ok(new
        {
            id = user.Id,
            fullName = user.FullName,
            email = user.Email,
            role = user.Role,
            cvUrl = user.CvUrl,
            createdAt = user.CreatedAt
        });
    }

    [Authorize]
    [HttpPost("cv")]
    public async Task<IActionResult> UploadCv(IFormFile file)
    {
        var userIdValue = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

        if (userIdValue == null)
            return Unauthorized();

        var user = await _context.Users
            .FirstOrDefaultAsync(u => u.Id == int.Parse(userIdValue));

        if (user == null)
            return NotFound(new { message = "User not found." });

        if (file == null || file.Length == 0)
            return BadRequest(new { message = "No file uploaded." });

        var extension = Path.GetExtension(file.FileName).ToLower();

        if (extension != ".pdf")
            return BadRequest(new { message = "Only PDF files are allowed." });

        if (file.Length > 5 * 1024 * 1024)
            return BadRequest(new { message = "PDF file must be smaller than 5MB." });

        var uploadsFolder = Path.Combine(
            Directory.GetCurrentDirectory(),
            "wwwroot",
            "uploads",
            "cv"
        );

        if (!Directory.Exists(uploadsFolder))
            Directory.CreateDirectory(uploadsFolder);

        var fileName = $"cv-user-{user.Id}-{Guid.NewGuid()}.pdf";
        var filePath = Path.Combine(uploadsFolder, fileName);

        using (var stream = new FileStream(filePath, FileMode.Create))
        {
            await file.CopyToAsync(stream);
        }

        user.CvUrl = $"/uploads/cv/{fileName}";

        await _context.SaveChangesAsync();

        return Ok(new
        {
            message = "CV uploaded successfully.",
            cvUrl = user.CvUrl
        });
    }

    [Authorize]
    [HttpDelete("cv")]
    public async Task<IActionResult> DeleteCv()
    {
        var userIdValue = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

        if (userIdValue == null)
            return Unauthorized();

        var user = await _context.Users
            .FirstOrDefaultAsync(u => u.Id == int.Parse(userIdValue));

        if (user == null)
            return NotFound(new { message = "User not found." });

        if (!string.IsNullOrEmpty(user.CvUrl))
        {
            var relativePath = user.CvUrl.TrimStart('/').Replace("/", Path.DirectorySeparatorChar.ToString());

            var filePath = Path.Combine(
                Directory.GetCurrentDirectory(),
                "wwwroot",
                relativePath
            );

            if (System.IO.File.Exists(filePath))
                System.IO.File.Delete(filePath);
        }

        user.CvUrl = null;

        await _context.SaveChangesAsync();

        return Ok(new { message = "CV deleted successfully." });
    }

    [Authorize]
    [HttpPut("{id}/role")]
    public async Task<IActionResult> UpdateUserRole(int id, [FromBody] UpdateUserRoleRequest request)
    {
        var user = await _context.Users.FindAsync(id);

        if (user == null)
            return NotFound(new { message = "User not found." });

        user.Role = request.Role;

        await _context.SaveChangesAsync();

        return Ok(new
        {
            message = "User role updated successfully.",
            user.Id,
            user.FullName,
            user.Email,
            user.Role
        });
    }
}