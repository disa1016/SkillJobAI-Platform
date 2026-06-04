using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SkillJobAI.Api.Data;
using SkillJobAI.Api.Entities;

namespace SkillJobAI.Api.Controllers;

[ApiController]
[Route("api/courses/{courseId}/skills")]
public class CourseSkillsController : ControllerBase
{
    private readonly AppDbContext _context;

    public CourseSkillsController(AppDbContext context)
    {
        _context = context;
    }

    [HttpGet]
    public async Task<IActionResult> GetCourseSkills(int courseId)
    {
        var courseExists = await _context.Courses.AnyAsync(c => c.Id == courseId);

        if (!courseExists)
            return NotFound(new { message = "Course not found." });

        var skills = await _context.CourseSkills
            .Where(cs => cs.CourseId == courseId)
            .Include(cs => cs.Skill)
            .Select(cs => new
            {
                id = cs.Skill.Id,
                name = cs.Skill.Name
            })
            .ToListAsync();

        return Ok(skills);
    }

    [Authorize]
    [HttpPost("{skillId}")]
    public async Task<IActionResult> AddSkillToCourse(int courseId, int skillId)
    {
        var courseExists = await _context.Courses.AnyAsync(c => c.Id == courseId);

        if (!courseExists)
            return NotFound(new { message = "Course not found." });

        var skillExists = await _context.Skills.AnyAsync(s => s.Id == skillId);

        if (!skillExists)
            return NotFound(new { message = "Skill not found." });

        var alreadyExists = await _context.CourseSkills
            .AnyAsync(cs => cs.CourseId == courseId && cs.SkillId == skillId);

        if (alreadyExists)
            return BadRequest(new { message = "Skill already added to this course." });

        var courseSkill = new CourseSkill
        {
            CourseId = courseId,
            SkillId = skillId
        };

        _context.CourseSkills.Add(courseSkill);
        await _context.SaveChangesAsync();

        return Ok(new
        {
            message = "Skill added to course successfully.",
            courseId,
            skillId
        });
    }

    [Authorize]
    [HttpDelete("{skillId}")]
    public async Task<IActionResult> RemoveSkillFromCourse(int courseId, int skillId)
    {
        var courseSkill = await _context.CourseSkills
            .FirstOrDefaultAsync(cs => cs.CourseId == courseId && cs.SkillId == skillId);

        if (courseSkill == null)
            return NotFound(new { message = "Skill is not assigned to this course." });

        _context.CourseSkills.Remove(courseSkill);
        await _context.SaveChangesAsync();

        return Ok(new
        {
            message = "Skill removed from course successfully.",
            courseId,
            skillId
        });
    }
}