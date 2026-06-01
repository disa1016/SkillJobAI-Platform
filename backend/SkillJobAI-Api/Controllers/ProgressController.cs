using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SkillJobAI.Api.Data;
using SkillJobAI.Api.Entities;

namespace SkillJobAI.Api.Controllers;

[ApiController]
[Route("api/progress")]
public class ProgressController : ControllerBase
{
    private readonly AppDbContext _context;

    public ProgressController(AppDbContext context)
    {
        _context = context;
    }

    [Authorize(Roles = "Student")]
    [HttpPost("complete")]
    public async Task<IActionResult> CompleteLesson(LessonProgress progress)
    {
        var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

        if (userId == null)
            return Unauthorized();

        var lessonExists = await _context.Lessons
            .AnyAsync(l => l.Id == progress.LessonId);

        if (!lessonExists)
            return NotFound(new { message = "Lesson not found" });

        var alreadyCompleted = await _context.LessonProgresses
            .AnyAsync(p =>
                p.UserId == int.Parse(userId) &&
                p.LessonId == progress.LessonId);

        if (alreadyCompleted)
            return BadRequest(new { message = "Lesson already completed" });

        progress.UserId = int.Parse(userId);
        progress.CompletedAt = DateTime.UtcNow;

        _context.LessonProgresses.Add(progress);
        await _context.SaveChangesAsync();

        return Ok(progress);
    }

    [Authorize(Roles = "Student")]
    [HttpGet("my")]
    public async Task<IActionResult> MyProgress()
    {
        var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

        if (userId == null)
            return Unauthorized();

        var progress = await _context.LessonProgresses
            .Where(p => p.UserId == int.Parse(userId))
            .Select(p => new
            {
                id = p.Id,
                lessonId = p.LessonId,
                completedAt = p.CompletedAt
            })
            .ToListAsync();

        return Ok(progress);
    }
}