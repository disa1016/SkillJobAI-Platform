using Microsoft.EntityFrameworkCore;
using SkillJobAI.Api.Data;

namespace SkillJobAI.Api.Services;

public class ApplicationMatchingService : IApplicationMatchingService
{
    private readonly AppDbContext _context;

    public ApplicationMatchingService(AppDbContext context)
    {
        _context = context;
    }

    public async Task<ApplicationMatchResult> GetMatchResultAsync(int jobId, int userId)
    {
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

        var matchPercentage = jobSkills.Count == 0
            ? 0
            : (int)Math.Round(((double)matchedSkills.Count / jobSkills.Count) * 100);

        var recommendedCourses = await _context.CourseSkills
            .Where(cs => missingSkills.Contains(cs.Skill.Name))
            .Include(cs => cs.Course)
            .Include(cs => cs.Skill)
            .Select(cs => new
            {
                id = cs.Course.Id,
                title = cs.Course.Title,
                skill = cs.Skill.Name
            })
            .Distinct()
            .Cast<object>()
            .ToListAsync();

        return new ApplicationMatchResult
        {
            MatchPercentage = matchPercentage,
            JobSkills = jobSkills,
            UserSkills = userSkills,
            MatchedSkills = matchedSkills,
            MissingSkills = missingSkills,
            RecommendedCourses = recommendedCourses
        };
    }
}