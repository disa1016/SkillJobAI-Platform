using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SkillJobAI.Api.Data;
using SkillJobAI.Api.Entities;
using SkillJobAI.Api.Models;
using SkillJobAI.Api.Models.Responses;

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
        var lessons = await _context.Lessons
            .OrderBy(l => l.OrderNumber)
            .Select(l => new LessonResponse
            {
                Id = l.Id,
                CourseId = l.CourseId,
                Title = l.Title,
                Content = l.Content,
                VideoUrl = l.VideoUrl ?? "",
                OrderNumber = l.OrderNumber,
                CreatedAt = l.CreatedAt
            })
            .ToListAsync();

        return Ok(lessons);
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetLesson(int id)
    {
        var lesson = await _context.Lessons
            .Where(l => l.Id == id)
            .Select(l => new LessonResponse
            {
                Id = l.Id,
                CourseId = l.CourseId,
                Title = l.Title,
                Content = l.Content,
                VideoUrl = l.VideoUrl ?? "",
                OrderNumber = l.OrderNumber,
                CreatedAt = l.CreatedAt
            })
            .FirstOrDefaultAsync();

        if (lesson == null)
            return NotFound(new { message = "Lesson not found." });

        return Ok(lesson);
    }

    [Authorize]
    [HttpPost]
    public async Task<IActionResult> CreateLesson([FromBody] LessonRequest request)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        var courseExists = await _context.Courses
            .AnyAsync(c => c.Id == request.CourseId);

        if (!courseExists)
            return BadRequest(new { message = "Course not found." });

        var lesson = new Lesson
        {
            Title = request.Title,
            Content = request.Content,
            VideoUrl = request.VideoUrl ?? "",
            OrderNumber = request.OrderNumber,
            CourseId = request.CourseId,
            CreatedAt = DateTime.UtcNow
        };

        _context.Lessons.Add(lesson);
        await _context.SaveChangesAsync();

        return Ok(new LessonResponse
        {
            Id = lesson.Id,
            CourseId = lesson.CourseId,
            Title = lesson.Title,
            Content = lesson.Content,
            VideoUrl = lesson.VideoUrl ?? "",
            OrderNumber = lesson.OrderNumber,
            CreatedAt = lesson.CreatedAt
        });
    }
}