using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SkillJobAI.Api.Services;

namespace SkillJobAI.Api.Controllers;

[ApiController]
[Route("api/courses/{courseId}/skills")]
public class CourseSkillsController : ControllerBase
{
    private readonly ICourseSkillService _courseSkillService;

    public CourseSkillsController(ICourseSkillService courseSkillService)
    {
        _courseSkillService = courseSkillService;
    }

    [HttpGet]
    public async Task<IActionResult> GetCourseSkills(int courseId)
    {
        var skills = await _courseSkillService.GetCourseSkillsAsync(courseId);

        if (skills == null)
            return NotFound(new { message = "Course not found." });

        return Ok(skills);
    }

    [Authorize]
    [HttpPost("{skillId}")]
    public async Task<IActionResult> AddSkillToCourse(int courseId, int skillId)
    {
        var result = await _courseSkillService.AddSkillToCourseAsync(courseId, skillId);

        if (result.Message == "Course not found.")
            return NotFound(result);

        if (result.Message == "Skill not found.")
            return NotFound(result);

        if (result.Message == "Skill already added to this course.")
            return BadRequest(result);

        return Ok(result);
    }

    [Authorize]
    [HttpDelete("{skillId}")]
    public async Task<IActionResult> RemoveSkillFromCourse(int courseId, int skillId)
    {
        var result = await _courseSkillService.RemoveSkillFromCourseAsync(courseId, skillId);

        if (result.Message == "Skill is not assigned to this course.")
            return NotFound(result);

        return Ok(result);
    }
}