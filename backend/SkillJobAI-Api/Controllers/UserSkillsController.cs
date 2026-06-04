using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SkillJobAI.Api.Data;
using SkillJobAI.Api.Entities;

namespace SkillJobAI.Api.Controllers;

[ApiController]
[Route("api/users/skills")]
public class UserSkillsController : ControllerBase
{
    private readonly AppDbContext _context;

    public UserSkillsController(AppDbContext context)
    {
        _context = context;
    }

    [Authorize]
    [HttpGet("my")]
    public async Task<IActionResult> GetMySkills()
    {
        var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

        if (userId == null)
            return Unauthorized();

        var skills = await _context.UserSkills
            .Where(us => us.UserId == int.Parse(userId))
            .Include(us => us.Skill)
            .Select(us => new
            {
                id = us.Skill.Id,
                name = us.Skill.Name
            })
            .ToListAsync();

        return Ok(skills);
    }

    [Authorize]
    [HttpPost("{skillId}")]
    public async Task<IActionResult> AddSkillToMe(int skillId)
    {
        var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

        if (userId == null)
            return Unauthorized();

        var userIdInt = int.Parse(userId);

        var skillExists = await _context.Skills
            .AnyAsync(s => s.Id == skillId);

        if (!skillExists)
            return NotFound(new { message = "Skill not found." });

        var alreadyExists = await _context.UserSkills
            .AnyAsync(us => us.UserId == userIdInt && us.SkillId == skillId);

        if (alreadyExists)
            return BadRequest(new { message = "Skill already added to your profile." });

        var userSkill = new UserSkill
        {
            UserId = userIdInt,
            SkillId = skillId
        };

        _context.UserSkills.Add(userSkill);
        await _context.SaveChangesAsync();

        return Ok(new
        {
            message = "Skill added to profile successfully.",
            userId = userIdInt,
            skillId
        });
    }

    [Authorize]
    [HttpDelete("{skillId}")]
    public async Task<IActionResult> RemoveSkillFromMe(int skillId)
    {
        var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

        if (userId == null)
            return Unauthorized();

        var userIdInt = int.Parse(userId);

        var userSkill = await _context.UserSkills
            .FirstOrDefaultAsync(us => us.UserId == userIdInt && us.SkillId == skillId);

        if (userSkill == null)
            return NotFound(new { message = "Skill is not assigned to your profile." });

        _context.UserSkills.Remove(userSkill);
        await _context.SaveChangesAsync();

        return Ok(new
        {
            message = "Skill removed from profile successfully.",
            userId = userIdInt,
            skillId
        });
    }
}