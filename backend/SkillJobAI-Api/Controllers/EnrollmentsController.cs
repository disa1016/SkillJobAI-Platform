using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SkillJobAI.Api.Data;
using SkillJobAI.Api.Entities;

namespace SkillJobAI.Api.Controllers;

[ApiController]
[Route("api/enrollments")]
public class EnrollmentsController : ControllerBase
{
    private readonly AppDbContext _context;

    public EnrollmentsController(AppDbContext context)
    {
        _context = context;
    }

    [Authorize]
    [HttpPost]
    public async Task<IActionResult> Enroll(Enrollment enrollment)
    {
        var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

        if (userId == null)
            return Unauthorized();

        var courseExists = await _context.Courses
            .AnyAsync(c => c.Id == enrollment.CourseId);

        if (!courseExists)
            return NotFound(new { message = "Course not found" });

        var alreadyEnrolled = await _context.Enrollments
            .AnyAsync(e =>
                e.UserId == int.Parse(userId) &&
                e.CourseId == enrollment.CourseId);

        if (alreadyEnrolled)
            return BadRequest(new { message = "Du bist bereits in diesem Kurs eingeschrieben." });

        enrollment.UserId = int.Parse(userId);
        enrollment.EnrolledAt = DateTime.UtcNow;
        enrollment.IsCompleted = false;

        _context.Enrollments.Add(enrollment);
        await _context.SaveChangesAsync();

        return Ok(enrollment);
    }
    [Authorize]
    [HttpGet("my")]
    public async Task<IActionResult> MyEnrollments()
    {
        var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

        if (userId == null)
            return Unauthorized();

        var enrollments = await _context.Enrollments
            .Where(e => e.UserId == int.Parse(userId))
            .Select(e => new
            {
                id = e.Id,
                courseId = e.CourseId,
                enrolledAt = e.EnrolledAt,
                isCompleted = e.IsCompleted,
                course = _context.Courses
                    .Where(c => c.Id == e.CourseId)
                    .Select(c => new
                    {
                        id = c.Id,
                        title = c.Title,
                        description = c.Description,
                        category = c.Category,
                        level = c.Level,
                        instructor = c.Instructor
                    })
                    .FirstOrDefault()
            })
            .ToListAsync();

        return Ok(enrollments);
    }


}