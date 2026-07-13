using SkillJobAI.Api.Models.Responses;

namespace SkillJobAI.Api.Services;

public interface IJobSkillService
{
    Task<List<SkillResponse>?> GetJobSkillsAsync(int jobId);

    Task<(bool Success, string? ErrorMessage)> AddSkillToJobAsync(int jobId, int skillId);

    Task<(bool Success, string? ErrorMessage)> RemoveSkillFromJobAsync(int jobId, int skillId);
}