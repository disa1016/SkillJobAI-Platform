using SkillJobAI.Api.Models;
using SkillJobAI.Api.Models.Responses;

namespace SkillJobAI.Api.Services;

public interface IApplicationService
{
    Task<ApplicationResponse?> CreateApplicationAsync(
        int userId,
        CreateApplicationRequest request);

    Task<List<ApplicationResponse>> GetMyApplicationsAsync(
        int userId);

    Task<ApplicationResponse?> GetApplicationByIdAsync(
        int id);

    Task<PagedResponse<ApplicationResponse>> GetApplicationsForJobAsync(
        int jobId,
        int page,
        int pageSize,
        string? search,
        string? status);

    Task<ApplicationResponse?> UpdateApplicationStatusAsync(
        int id,
        string status);

    Task<int?> GetApplicationCompanyIdAsync(
        int applicationId);

    Task<int?> GetJobCompanyIdAsync(
        int jobId);

    Task<ApplicationFileDownloadResponse?> GetApplicationFileAsync(
        int applicationId,
        string fileType);
}