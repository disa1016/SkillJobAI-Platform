using SkillJobAI.Api.Models.Responses;

namespace SkillJobAI.Api.Services;

public interface IUserSkillService
{
    Task<List<SkillResponse>> GetMySkillsAsync(int userId);

    Task<(bool Success, string? ErrorMessage)> AddSkillToUserAsync(
        int userId,
        int skillId);

    Task<(bool Success, string? ErrorMessage)> RemoveSkillFromUserAsync(
        int userId,
        int skillId);
}