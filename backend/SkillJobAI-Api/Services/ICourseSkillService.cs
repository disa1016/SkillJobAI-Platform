using SkillJobAI.Api.Models.Responses;

namespace SkillJobAI.Api.Services;

public interface ICourseSkillService
{
    Task<List<CourseSkillResponse>?> GetCourseSkillsAsync(int courseId);
    Task<CourseSkillActionResponse> AddSkillToCourseAsync(int courseId, int skillId);
    Task<CourseSkillActionResponse> RemoveSkillFromCourseAsync(int courseId, int skillId);
}