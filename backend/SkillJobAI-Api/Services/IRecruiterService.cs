using SkillJobAI.Api.Models.Responses;

namespace SkillJobAI.Api.Services;

public interface IRecruiterService
{
    Task<RecruiterDashboardResponse> GetDashboardAsync(int userId, bool isAdmin);
}