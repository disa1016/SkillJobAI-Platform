using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SkillJobAI.Api.Data;
using SkillJobAI.Api.Entities;

namespace SkillJobAI.Api.Controllers;

[ApiController]
[Route("api/skills")]
public class SkillsController : ControllerBase
{
    private readonly AppDbContext _context;

    public SkillsController(AppDbContext context)
    {
        _context = context;
    }

    [HttpGet]
    public async Task<IActionResult> GetSkills()
    {
        var skills = await _context.Skills.ToListAsync();
        return Ok(skills);
    }

    [Authorize]
    [HttpPost]
    public async Task<IActionResult> CreateSkill(Skill skill)
    {
        var exists = await _context.Skills
            .AnyAsync(s => s.Name.ToLower() == skill.Name.ToLower());

        if (exists)
        {
            return BadRequest(new
            {
                message = "Skill already exists."
            });
        }

        _context.Skills.Add(skill);
        await _context.SaveChangesAsync();

        return Ok(skill);
    }
}