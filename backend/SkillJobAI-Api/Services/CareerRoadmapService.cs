using Microsoft.EntityFrameworkCore;
using SkillJobAI.Api.Data;
using SkillJobAI.Api.Entities;
using SkillJobAI.Api.Models.Responses;

namespace SkillJobAI.Api.Services;

public class CareerRoadmapService : ICareerRoadmapService
{
    private readonly AppDbContext _context;

    public CareerRoadmapService(AppDbContext context)
    {
        _context = context;
    }

    public async Task<List<CareerGoalResponse>> GetCareerGoalsAsync()
    {
        return await _context.CareerGoals
            .Select(g => new CareerGoalResponse
            {
                Id = g.Id,
                Name = g.Name,
                Description = g.Description,
                DurationMonths = g.DurationMonths
            })
            .ToListAsync();
    }

    public async Task<SelectCareerGoalResponse?> SelectCareerGoalAsync(int userId, int goalId)
    {
        var goalExists = await _context.CareerGoals
            .AnyAsync(g => g.Id == goalId);

        if (!goalExists)
            return null;

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

        return new SelectCareerGoalResponse
        {
            Message = "Career goal selected successfully.",
            GoalId = goalId
        };
    }

    public async Task<CareerRoadmapResponse> GetMyCareerRoadmapAsync(int userId)
    {
        var userCareerGoal = await _context.UserCareerGoals
            .Include(ucg => ucg.CareerGoal)
            .FirstOrDefaultAsync(ucg => ucg.UserId == userId);

        if (userCareerGoal == null)
        {
            return new CareerRoadmapResponse
            {
                HasCareerGoal = false,
                Message = "No career goal selected."
            };
        }

        var requiredSkills = await _context.CareerGoalSkills
            .Where(cgs => cgs.CareerGoalId == userCareerGoal.CareerGoalId)
            .Include(cgs => cgs.Skill)
            .Select(cgs => new CareerRoadmapSkillResponse
            {
                Id = cgs.Skill.Id,
                Name = cgs.Skill.Name,
                MonthNumber = cgs.MonthNumber
            })
            .ToListAsync();

        var userSkills = await _context.UserSkills
            .Where(us => us.UserId == userId)
            .Include(us => us.Skill)
            .Select(us => us.Skill.Name)
            .ToListAsync();

        var completedSkills = requiredSkills
            .Where(rs => userSkills.Contains(rs.Name))
            .ToList();

        var missingSkills = requiredSkills
            .Where(rs => !userSkills.Contains(rs.Name))
            .ToList();

        var progressPercentage = requiredSkills.Count == 0
            ? 0
            : (int)Math.Round(((double)completedSkills.Count / requiredSkills.Count) * 100);

        var missingSkillNames = missingSkills.Select(ms => ms.Name).ToList();

        var recommendedCourses = await _context.CourseSkills
            .Where(cs => missingSkillNames.Contains(cs.Skill.Name))
            .Include(cs => cs.Course)
            .Include(cs => cs.Skill)
            .Select(cs => new CareerRoadmapCourseResponse
            {
                Id = cs.Course.Id,
                Title = cs.Course.Title,
                Skill = cs.Skill.Name
            })
            .Distinct()
            .ToListAsync();

        var phases = requiredSkills
            .GroupBy(s => s.MonthNumber)
            .OrderBy(g => g.Key)
            .Select(g => new CareerRoadmapPhaseResponse
            {
                Month = g.Key,
                Skills = g.Select(s => new CareerRoadmapPhaseSkillResponse
                {
                    Id = s.Id,
                    Name = s.Name,
                    IsCompleted = userSkills.Contains(s.Name)
                }).ToList()
            })
            .ToList();

        return new CareerRoadmapResponse
        {
            HasCareerGoal = true,
            Goal = new CareerRoadmapGoalResponse
            {
                Id = userCareerGoal.CareerGoal.Id,
                Name = userCareerGoal.CareerGoal.Name,
                Description = userCareerGoal.CareerGoal.Description,
                DurationMonths = userCareerGoal.CareerGoal.DurationMonths,
                StartedAt = userCareerGoal.StartedAt
            },
            ProgressPercentage = progressPercentage,
            TotalSkills = requiredSkills.Count,
            CompletedSkillsCount = completedSkills.Count,
            MissingSkillsCount = missingSkills.Count,
            CompletedSkills = completedSkills,
            MissingSkills = missingSkills,
            RecommendedCourses = recommendedCourses,
            Phases = phases
        };
    }
}