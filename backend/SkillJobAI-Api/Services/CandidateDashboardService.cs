using Microsoft.EntityFrameworkCore;
using SkillJobAI.Api.Data;
using SkillJobAI.Api.Models.Responses;

namespace SkillJobAI.Api.Services;

public class CandidateDashboardService : ICandidateDashboardService
{
    private readonly AppDbContext _context;

    public CandidateDashboardService(AppDbContext context)
    {
        _context = context;
    }

    public async Task<CandidateDashboardResponse> GetDashboardAsync(int userId)
    {
        var applicationsCount = await _context.Applications.CountAsync(a => a.UserId == userId);

        var enrollmentsCount = await _context.Enrollments.CountAsync(e => e.UserId == userId);

        var completedLessonsCount = await _context.LessonProgresses.CountAsync(p => p.UserId == userId);

        var userSkills = await _context.UserSkills
            .Where(us => us.UserId == userId)
            .Include(us => us.Skill)
            .Select(us => us.Skill.Name)
            .ToListAsync();

        var allJobSkills = await _context.JobSkills
            .Include(js => js.Skill)
            .Select(js => js.Skill.Name)
            .ToListAsync();

        var missingSkills = allJobSkills
            .Except(userSkills)
            .Distinct()
            .Take(5)
            .ToList();

        var recommendedCourses = await _context.CourseSkills
      .Where(cs => missingSkills.Contains(cs.Skill.Name))
      .Include(cs => cs.Course)
      .Include(cs => cs.Skill)
      .Select(cs => new CandidateRecommendedCourseResponse
      {
          Id = cs.Course!.Id,
          Title = cs.Course.Title,
          Skill = cs.Skill.Name
      })
      .Distinct()
      .Take(5)
      .ToListAsync();

        var jobs = await _context.Jobs
            .Include(j => j.Company)
            .ToListAsync();

        var topJobMatches = new List<TopJobMatchResponse>();

        foreach (var job in jobs)
        {
            var jobSkills = await _context.JobSkills
                .Where(js => js.JobId == job.Id)
                .Include(js => js.Skill)
                .Select(js => js.Skill.Name)
                .ToListAsync();

            if (jobSkills.Count == 0)
                continue;

            var matchedSkills = jobSkills.Intersect(userSkills).ToList();

            var missingJobSkills = jobSkills.Except(userSkills).ToList();

            var matchPercentage = (int)Math.Round(
                ((double)matchedSkills.Count / jobSkills.Count) * 100
            );

            topJobMatches.Add(new TopJobMatchResponse
            {
                Id = job.Id,
                Title = job.Title,
                Location = job.Location,
                Salary = job.Salary,
                Company = job.Company == null ? null : new JobMatchCompanyResponse
                {
                    Id = job.Company.Id,
                    Name = job.Company.Name
                },
                MatchPercentage = matchPercentage,
                MatchedSkills = matchedSkills,
                MissingSkills = missingJobSkills
            });
        }

        return new CandidateDashboardResponse
        {
            ApplicationsCount = applicationsCount,
            EnrollmentsCount = enrollmentsCount,
            CompletedLessonsCount = completedLessonsCount,
            UserSkills = userSkills,
            MissingSkills = missingSkills,
            RecommendedCourses = recommendedCourses,
            TopJobMatches = topJobMatches
                .OrderByDescending(j => j.MatchPercentage)
                .Take(5)
                .ToList()
        };
    }
}