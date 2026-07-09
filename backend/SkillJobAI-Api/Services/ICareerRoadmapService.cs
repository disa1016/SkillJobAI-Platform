using SkillJobAI.Api.Models.Responses;

namespace SkillJobAI.Api.Services;

public interface ICareerRoadmapService
{
    Task<List<CareerGoalResponse>> GetCareerGoalsAsync();
    Task<SelectCareerGoalResponse?> SelectCareerGoalAsync(int userId, int goalId);
    Task<CareerRoadmapResponse> GetMyCareerRoadmapAsync(int userId);
}