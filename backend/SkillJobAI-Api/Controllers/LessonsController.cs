using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SkillJobAI.Api.Data;
using SkillJobAI.Api.Entities;

namespace SkillJobAI.Api.Controllers;

[ApiController]
[Route("api/lessons")]
public class LessonsController : ControllerBase
{
    private readonly AppDbContext _context;

    public LessonsController(AppDbContext context)
    {
        _context = context;
    }

    [HttpGet]
    public async Task<IActionResult> GetLessons()
    {
        var lessons = await _context.Lessons.ToListAsync();
        return Ok(lessons);
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetLesson(int id)
    {
        var lesson = await _context.Lessons.FindAsync(id);

        if (lesson == null)
            return NotFound();

        return Ok(lesson);
    }

    [Authorize]
    [HttpPost]
    public async Task<IActionResult> CreateLesson(Lesson lesson)
    {
        lesson.CreatedAt = DateTime.UtcNow;

        _context.Lessons.Add(lesson);
        await _context.SaveChangesAsync();

        return Ok(lesson);
    }
}