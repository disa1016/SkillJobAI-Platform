using Microsoft.EntityFrameworkCore;
using SkillJobAI.Api.Data;
using SkillJobAI.Api.Models.Responses;

namespace SkillJobAI.Api.Services;

public class RecruiterService : IRecruiterService
{
    private readonly AppDbContext _context;

    public RecruiterService(AppDbContext context)
    {
        _context = context;
    }

    public async Task<RecruiterDashboardResponse> GetDashboardAsync(int userId, bool isAdmin)
    {
        var companyIdsQuery = _context.CompanyMembers
            .Where(cm => cm.UserId == userId)
            .Select(cm => cm.CompanyId);

        if (isAdmin)
        {
            companyIdsQuery = _context.Companies.Select(c => c.Id);
        }

        var companyIds = await companyIdsQuery.ToListAsync();

        var jobsQuery = _context.Jobs
            .Where(j => j.CompanyId != null && companyIds.Contains(j.CompanyId.Value));

        var jobIds = await jobsQuery
            .Select(j => j.Id)
            .ToListAsync();

        var applicationsQuery = _context.Applications
            .Where(a => jobIds.Contains(a.JobId));

        var recentApplications = await applicationsQuery
            .OrderByDescending(a => a.CreatedAt)
            .Take(5)
            .Select(a => new RecruiterRecentApplicationResponse
            {
                Id = a.Id,
                Status = a.Status,
                CreatedAt = a.CreatedAt,
                CoverLetter = a.CoverLetter,
                Job = _context.Jobs
                    .Where(j => j.Id == a.JobId)
                    .Select(j => new RecruiterJobInfoResponse
                    {
                        Id = j.Id,
                        Title = j.Title,
                        Company = j.Company != null ? j.Company.Name : null
                    })
                    .FirstOrDefault(),
                Candidate = _context.Users
                    .Where(u => u.Id == a.UserId)
                    .Select(u => new RecruiterCandidateInfoResponse
                    {
                        Id = u.Id,
                        FullName = u.FullName,
                        Email = u.Email
                    })
                    .FirstOrDefault()
            })
            .ToListAsync();

        var topJobsByApplications = await applicationsQuery
            .GroupBy(a => a.JobId)
            .Select(g => new RecruiterTopJobResponse
            {
                JobId = g.Key,
                ApplicationsCount = g.Count(),
                Job = _context.Jobs
                    .Where(j => j.Id == g.Key)
                    .Select(j => new RecruiterJobInfoResponse
                    {
                        Id = j.Id,
                        Title = j.Title,
                        Company = j.Company != null ? j.Company.Name : null
                    })
                    .FirstOrDefault()
            })
            .OrderByDescending(x => x.ApplicationsCount)
            .Take(5)
            .ToListAsync();

        return new RecruiterDashboardResponse
        {
            TotalCompanies = companyIds.Count,
            TotalJobs = await jobsQuery.CountAsync(),
            TotalApplications = await applicationsQuery.CountAsync(),
            PendingApplications = await applicationsQuery.CountAsync(a => a.Status == "Pending"),
            ReviewedApplications = await applicationsQuery.CountAsync(a => a.Status == "Reviewed"),
            AcceptedApplications = await applicationsQuery.CountAsync(a => a.Status == "Accepted"),
            RejectedApplications = await applicationsQuery.CountAsync(a => a.Status == "Rejected"),
            RecentApplications = recentApplications,
            TopJobsByApplications = topJobsByApplications
        };
    }
}