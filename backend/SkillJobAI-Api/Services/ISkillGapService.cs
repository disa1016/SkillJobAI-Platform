using SkillJobAI.Api.Models.Responses;

namespace SkillJobAI.Api.Services;

public interface ISkillGapService
{
    Task<SkillGapResponse?> GetSkillGapAsync(int userId, int jobId);
}