using SkillJobAI.Api.Models;
using SkillJobAI.Api.Models.Responses;

namespace SkillJobAI.Api.Services;

public interface ISkillService
{
    Task<List<SkillResponse>> GetSkillsAsync();
    Task<SkillResponse?> CreateSkillAsync(SkillRequest request);
}