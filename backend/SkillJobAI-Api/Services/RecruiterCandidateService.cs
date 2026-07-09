using Microsoft.EntityFrameworkCore;
using SkillJobAI.Api.Data;
using SkillJobAI.Api.Models.Responses;

namespace SkillJobAI.Api.Services;

public class RecruiterCandidateService : IRecruiterCandidateService
{
    private readonly AppDbContext _context;

    public RecruiterCandidateService(AppDbContext context)
    {
        _context = context;
    }

    public async Task<List<RecruiterCandidateResponse>> GetCandidatesAsync(string? skill)
    {
        var candidates = await _context.Users
            .Where(u => u.Role == "Candidate" || u.Role == "Student")
            .Select(u => new RecruiterCandidateResponse
            {
                Id = u.Id,
                FullName = u.FullName,
                Email = u.Email,
                CreatedAt = u.CreatedAt
            })
            .ToListAsync();

        var result = new List<RecruiterCandidateResponse>();

        foreach (var candidate in candidates)
        {
            var skills = await _context.UserSkills
                .Where(us => us.UserId == candidate.Id)
                .Include(us => us.Skill)
                .Select(us => us.Skill.Name)
                .ToListAsync();

            if (!string.IsNullOrWhiteSpace(skill))
            {
                var hasSkill = skills.Any(s =>
                    s.ToLower().Contains(skill.ToLower()));

                if (!hasSkill)
                    continue;
            }

            var applicationsCount = await _context.Applications
                .CountAsync(a => a.UserId == candidate.Id);

            var acceptedApplications = await _context.Applications
                .CountAsync(a => a.UserId == candidate.Id && a.Status == "Accepted");

            var rejectedApplications = await _context.Applications
                .CountAsync(a => a.UserId == candidate.Id && a.Status == "Rejected");

            candidate.Skills = skills;
            candidate.SkillsCount = skills.Count;
            candidate.ApplicationsCount = applicationsCount;
            candidate.AcceptedApplications = acceptedApplications;
            candidate.RejectedApplications = rejectedApplications;

            result.Add(candidate);
        }

        return result
            .OrderByDescending(c => c.SkillsCount)
            .ToList();
    }

    public async Task<RecruiterCandidateDetailResponse?> GetCandidateAsync(int id)
    {
        var candidate = await _context.Users
            .Where(u =>
                u.Id == id &&
                (u.Role == "Candidate" || u.Role == "Student"))
            .Select(u => new RecruiterCandidateDetailResponse
            {
                Id = u.Id,
                FullName = u.FullName,
                Email = u.Email,
                CvUrl = u.CvUrl,
                CreatedAt = u.CreatedAt
            })
            .FirstOrDefaultAsync();

        if (candidate == null)
            return null;

        var skills = await _context.UserSkills
            .Where(us => us.UserId == id)
            .Include(us => us.Skill)
            .Select(us => us.Skill.Name)
            .ToListAsync();

        var applications = await _context.Applications
            .Where(a => a.UserId == id)
            .OrderByDescending(a => a.CreatedAt)
            .Select(a => new RecruiterCandidateApplicationResponse
            {
                Id = a.Id,
                JobId = a.JobId,
                CoverLetter = a.CoverLetter,
                Status = a.Status,
                CvFileUrl = a.CvFileUrl,
                CertificateFileUrl = a.CertificateFileUrl,
                PortfolioFileUrl = a.PortfolioFileUrl,
                CreatedAt = a.CreatedAt,
                Job = _context.Jobs
                    .Where(j => j.Id == a.JobId)
                    .Select(j => new RecruiterCandidateJobResponse
                    {
                        Id = j.Id,
                        Title = j.Title,
                        Company = j.Company != null ? j.Company.Name : null,
                        Location = j.Location,
                        Salary = j.Salary
                    })
                    .FirstOrDefault()
            })
            .ToListAsync();

        candidate.Skills = skills;
        candidate.SkillsCount = skills.Count;
        candidate.ApplicationsCount = applications.Count;
        candidate.AcceptedApplications = applications.Count(a => a.Status == "Accepted");
        candidate.RejectedApplications = applications.Count(a => a.Status == "Rejected");
        candidate.Applications = applications;

        return candidate;
    }
}