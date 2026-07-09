using SkillJobAI.Api.Models.Responses;

namespace SkillJobAI.Api.Services;

public interface ICandidateDashboardService
{
    Task<CandidateDashboardResponse> GetDashboardAsync(int userId);
}