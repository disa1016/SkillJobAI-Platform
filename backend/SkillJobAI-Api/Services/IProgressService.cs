using SkillJobAI.Api.Models;
using SkillJobAI.Api.Models.Responses;

namespace SkillJobAI.Api.Services;

public interface IProgressService
{
    Task<(bool Success, string? ErrorMessage, LessonProgressResponse? Progress)> CompleteLessonAsync(
        int userId,
        LessonProgressRequest request);

    Task<List<LessonProgressResponse>> GetMyProgressAsync(int userId);
}