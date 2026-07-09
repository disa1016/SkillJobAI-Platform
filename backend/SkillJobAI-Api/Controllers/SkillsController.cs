using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SkillJobAI.Api.Models;
using SkillJobAI.Api.Services;

namespace SkillJobAI.Api.Controllers;

[ApiController]
[Route("api/skills")]
public class SkillsController : ControllerBase
{
    private readonly ISkillService _skillService;

    public SkillsController(ISkillService skillService)
    {
        _skillService = skillService;
    }

    [HttpGet]
    public async Task<IActionResult> GetSkills()
    {
        var skills = await _skillService.GetSkillsAsync();
        return Ok(skills);
    }

    [Authorize]
    [HttpPost]
    public async Task<IActionResult> CreateSkill(SkillRequest request)
    {
        var skill = await _skillService.CreateSkillAsync(request);

        if (skill == null)
            return BadRequest(new { message = "Skill already exists." });

        return Ok(skill);
    }
}