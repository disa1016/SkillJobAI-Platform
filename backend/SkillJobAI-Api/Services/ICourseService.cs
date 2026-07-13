using SkillJobAI.Api.Models;
using SkillJobAI.Api.Models.Responses;

namespace SkillJobAI.Api.Services;

public interface ICourseService
{
    Task<PagedResponse<CourseResponse>> GetCoursesAsync(
        int page,
        int pageSize,
        string? search);

    Task<CourseResponse?> GetCourseByIdAsync(int id);

    Task<CourseResponse> CreateCourseAsync(CourseRequest request);

    Task<bool> DeleteCourseAsync(int id);
}