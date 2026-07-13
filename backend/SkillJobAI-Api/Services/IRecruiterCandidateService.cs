using SkillJobAI.Api.Models.Responses;

namespace SkillJobAI.Api.Services;

public interface IRecruiterCandidateService
{
    Task<List<RecruiterCandidateResponse>> GetCandidatesAsync(string? skill);
    Task<RecruiterCandidateDetailResponse?> GetCandidateAsync(int id);
}