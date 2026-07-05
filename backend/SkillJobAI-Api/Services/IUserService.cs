using SkillJobAI.Api.Models;
using SkillJobAI.Api.Models.Responses;

namespace SkillJobAI.Api.Services;

public interface IUserService
{
    Task<UserResponse?> GetProfileAsync(int userId);

    Task<(bool Success, string? ErrorMessage, string? CvUrl)> UploadCvAsync(
        int userId,
        IFormFile file);

    Task<bool> DeleteCvAsync(int userId);

    Task<UserResponse?> UpdateUserRoleAsync(int id, UpdateUserRoleRequest request);
}