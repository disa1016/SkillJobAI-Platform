using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SkillJobAI.Api.Data;
using SkillJobAI.Api.Entities;

namespace SkillJobAI.Api.Controllers;

[ApiController]
public class CareerRoadmapController : ControllerBase
{
    private readonly AppDbContext _context;

    public CareerRoadmapController(AppDbContext context)
    {
        _context = context;
    }

    [Authorize]
    [HttpGet("api/career-goals")]
    public async Task<IActionResult> GetCareerGoals()
    {
        var goals = await _context.CareerGoals
            .Select(g => new
            {
                id = g.Id,
                name = g.Name,
                description = g.Description,
                durationMonths = g.DurationMonths
            })
            .ToListAsync();

        return Ok(goals);
    }

    [Authorize]
    [HttpPost("api/career-goals/select/{goalId}")]
    public async Task<IActionResult> SelectCareerGoal(int goalId)
    {
        var userIdValue = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

        if (userIdValue == null)
            return Unauthorized();

        var userId = int.Parse(userIdValue);

        var goalExists = await _context.CareerGoals
            .AnyAsync(g => g.Id == goalId);

        if (!goalExists)
            return NotFound(new { message = "Career goal not found." });

        var existingGoal = await _context.UserCareerGoals
            .FirstOrDefaultAsync(g => g.UserId == userId);

        if (existingGoal != null)
        {
            existingGoal.CareerGoalId = goalId;
            existingGoal.StartedAt = DateTime.UtcNow;
        }
        else
        {
            var userCareerGoal = new UserCareerGoal
            {
                UserId = userId,
                CareerGoalId = goalId,
                StartedAt = DateTime.UtcNow
            };

            _context.UserCareerGoals.Add(userCareerGoal);
        }

        await _context.SaveChangesAsync();

        return Ok(new
        {
            message = "Career goal selected successfully.",
            goalId
        });
    }

    [Authorize]
    [HttpGet("api/career-roadmap/my")]
    public async Task<IActionResult> GetMyCareerRoadmap()
    {
        var userIdValue = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

        if (userIdValue == null)
            return Unauthorized();

        var userId = int.Parse(userIdValue);

        var userCareerGoal = await _context.UserCareerGoals
            .Include(ucg => ucg.CareerGoal)
            .FirstOrDefaultAsync(ucg => ucg.UserId == userId);

        if (userCareerGoal == null)
        {
            return Ok(new
            {
                hasCareerGoal = false,
                message = "No career goal selected."
            });
        }

        var requiredSkills = await _context.CareerGoalSkills
            .Where(cgs => cgs.CareerGoalId == userCareerGoal.CareerGoalId)
            .Include(cgs => cgs.Skill)
            .Select(cgs => new
            {
                id = cgs.Skill.Id,
                name = cgs.Skill.Name,
                monthNumber = cgs.MonthNumber
            })
            .ToListAsync();

        var userSkills = await _context.UserSkills
            .Where(us => us.UserId == userId)
            .Include(us => us.Skill)
            .Select(us => us.Skill.Name)
            .ToListAsync();

        var completedSkills = requiredSkills
            .Where(rs => userSkills.Contains(rs.name))
            .ToList();

        var missingSkills = requiredSkills
            .Where(rs => !userSkills.Contains(rs.name))
            .ToList();

        var progressPercentage = requiredSkills.Count == 0
            ? 0
            : (int)Math.Round(((double)completedSkills.Count / requiredSkills.Count) * 100);

        var recommendedCourses = await _context.CourseSkills
            .Where(cs => missingSkills.Select(ms => ms.name).Contains(cs.Skill.Name))
            .Include(cs => cs.Course)
            .Include(cs => cs.Skill)
            .Select(cs => new
            {
                id = cs.Course.Id,
                title = cs.Course.Title,
                skill = cs.Skill.Name
            })
            .Distinct()
            .ToListAsync();

        var phases = requiredSkills
            .GroupBy(s => s.monthNumber)
            .OrderBy(g => g.Key)
            .Select(g => new
            {
                month = g.Key,
                skills = g.Select(s => new
                {
                    s.id,
                    s.name,
                    isCompleted = userSkills.Contains(s.name)
                })
                .ToList()
            })
            .ToList();

        return Ok(new
        {
            hasCareerGoal = true,
            goal = new
            {
                id = userCareerGoal.CareerGoal.Id,
                name = userCareerGoal.CareerGoal.Name,
                description = userCareerGoal.CareerGoal.Description,
                durationMonths = userCareerGoal.CareerGoal.DurationMonths,
                startedAt = userCareerGoal.StartedAt
            },
            progressPercentage,
            totalSkills = requiredSkills.Count,
            completedSkillsCount = completedSkills.Count,
            missingSkillsCount = missingSkills.Count,
            completedSkills,
            missingSkills,
            recommendedCourses,
            phases
        });
    }
}