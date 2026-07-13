using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SkillJobAI.Api.Models;
using SkillJobAI.Api.Services;

namespace SkillJobAI.Api.Controllers;

[ApiController]
[Route("api/lessons")]
public class LessonsController : ControllerBase
{
    private readonly ILessonService _lessonService;

    public LessonsController(ILessonService lessonService)
    {
        _lessonService = lessonService;
    }

    [HttpGet]
    public async Task<IActionResult> GetLessons()
    {
        var lessons = await _lessonService.GetLessonsAsync();
        return Ok(lessons);
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetLesson(int id)
    {
        var lesson = await _lessonService.GetLessonByIdAsync(id);

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

        var lesson = await _lessonService.CreateLessonAsync(request);

        if (lesson == null)
            return BadRequest(new { message = "Course not found." });

        return Ok(lesson);
    }
}