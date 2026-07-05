using SkillJobAI.Api.Models;
using SkillJobAI.Api.Models.Responses;

namespace SkillJobAI.Api.Services;

public interface IJobService
{
    Task<PagedResponse<JobResponse>> GetJobsAsync(int page, int pageSize, string? search);
    Task<JobResponse?> GetJobByIdAsync(int id);
    Task<JobResponse?> CreateJobAsync(JobRequest request);
    Task<JobResponse?> UpdateJobAsync(int id, JobRequest request);
    Task<bool> DeleteJobAsync(int id);
    Task<bool> CompanyExistsAsync(int companyId);
    Task<int?> GetJobCompanyIdAsync(int jobId);
}