using SkillJobAI.Api.Models;
using SkillJobAI.Api.Models.Responses;

namespace SkillJobAI.Api.Services;

public interface ILessonService
{
    Task<List<LessonResponse>> GetLessonsAsync();

    Task<LessonResponse?> GetLessonByIdAsync(int id);

    Task<LessonResponse?> CreateLessonAsync(LessonRequest request);
}