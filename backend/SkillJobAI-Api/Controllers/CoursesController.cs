using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SkillJobAI.Api.Data;
using SkillJobAI.Api.Entities;

namespace SkillJobAI.Api.Controllers;

[ApiController]
[Route("api/courses")]
public class CoursesController : ControllerBase
{
    private readonly AppDbContext _context;

    public CoursesController(AppDbContext context)
    {
        _context = context;
    }

    // Alle Kurse abrufen
    [HttpGet]
    public async Task<IActionResult> GetCourses()
    {
        var courses = await _context.Courses
            .Select(c => new
            {
                id = c.Id,
                title = c.Title,
                description = c.Description,
                category = c.Category,
                level = c.Level,
                instructor = c.Instructor,
                createdAt = c.CreatedAt
            })
            .ToListAsync();

        return Ok(courses);
    }

    // Einzelnen Kurs mit Lessons abrufen
    [HttpGet("{id}")]
    public async Task<IActionResult> GetCourse(int id)
    {
        var course = await _context.Courses
            .Include(c => c.Lessons)
            .FirstOrDefaultAsync(c => c.Id == id);

        if (course == null)
            return NotFound();

        return Ok(new
        {
            id = course.Id,
            title = course.Title,
            description = course.Description,
            category = course.Category,
            level = course.Level,
            instructor = course.Instructor,
            createdAt = course.CreatedAt,
            lessons = course.Lessons
                .OrderBy(l => l.OrderNumber)
                .Select(l => new
                {
                    id = l.Id,
                    courseId = l.CourseId,
                    title = l.Title,
                    content = l.Content,
                    videoUrl = l.VideoUrl,
                    orderNumber = l.OrderNumber,
                    createdAt = l.CreatedAt
                })
        });
    }

    // Kurs erstellen - nur Instructor
    [Authorize(Roles = "Instructor")]
    [HttpPost]
    public async Task<IActionResult> CreateCourse(Course course)
    {
        course.CreatedAt = DateTime.UtcNow;

        _context.Courses.Add(course);
        await _context.SaveChangesAsync();

        return Ok(new
        {
            id = course.Id,
            title = course.Title,
            description = course.Description,
            category = course.Category,
            level = course.Level,
            instructor = course.Instructor,
            createdAt = course.CreatedAt
        });
    }

    // Kurs löschen - nur Admin
    [Authorize(Roles = "Admin")]
    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteCourse(int id)
    {
        var course = await _context.Courses.FindAsync(id);

        if (course == null)
            return NotFound();

        _context.Courses.Remove(course);
        await _context.SaveChangesAsync();

        return Ok(new
        {
            message = "Course deleted successfully"
        });
    }
}