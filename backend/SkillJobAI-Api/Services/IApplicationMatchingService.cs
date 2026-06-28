namespace SkillJobAI.Api.Services;

public interface IApplicationMatchingService
{
    Task<ApplicationMatchResult> GetMatchResultAsync(int jobId, int userId);
}