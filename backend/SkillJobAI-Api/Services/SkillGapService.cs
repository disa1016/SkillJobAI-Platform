using Microsoft.EntityFrameworkCore;
using SkillJobAI.Api.Data;
using SkillJobAI.Api.Models.Responses;

namespace SkillJobAI.Api.Services;

public class SkillGapService : ISkillGapService
{
    private readonly AppDbContext _context;

    public SkillGapService(AppDbContext context)
    {
        _context = context;
    }

    public async Task<SkillGapResponse?> GetSkillGapAsync(int userId, int jobId)
    {
        var job = await _context.Jobs
            .FirstOrDefaultAsync(j => j.Id == jobId);

        if (job == null)
            return null;

        var jobSkills = await _context.JobSkills
            .Where(js => js.JobId == jobId)
            .Include(js => js.Skill)
            .Select(js => js.Skill.Name)
            .ToListAsync();

        var userSkills = await _context.UserSkills
            .Where(us => us.UserId == userId)
            .Include(us => us.Skill)
            .Select(us => us.Skill.Name)
            .ToListAsync();

        var matchedSkills = jobSkills
            .Intersect(userSkills)
            .ToList();

        var missingSkills = jobSkills
            .Except(userSkills)
            .ToList();

        var hasJobSkills = jobSkills.Any();

        var matchPercentage = hasJobSkills
            ? (int)Math.Round(((double)matchedSkills.Count / jobSkills.Count) * 100)
            : 0;

        var recommendedCourses = await _context.CourseSkills
            .Where(cs => missingSkills.Contains(cs.Skill.Name))
            .Include(cs => cs.Course)
            .Select(cs => new RecommendedCourseResponse
            {
                Id = cs.Course.Id,
                Title = cs.Course.Title
            })
            .Distinct()
            .ToListAsync();

        return new SkillGapResponse
        {
            JobId = job.Id,
            JobTitle = job.Title,
            HasJobSkills = hasJobSkills,
            MatchPercentage = matchPercentage,
            JobSkills = jobSkills,
            UserSkills = userSkills,
            MatchedSkills = matchedSkills,
            MissingSkills = missingSkills,
            RecommendedCourses = recommendedCourses
        };
    }
}