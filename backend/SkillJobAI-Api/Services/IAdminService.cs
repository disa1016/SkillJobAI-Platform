using SkillJobAI.Api.Models;
using SkillJobAI.Api.Models.Responses;

namespace SkillJobAI.Api.Services;

public interface IAdminService
{
    Task<AdminDashboardResponse> GetDashboardAsync();
    Task<List<AdminUserResponse>> GetUsersAsync();
    Task<AdminUserResponse?> UpdateUserRoleAsync(int id, UpdateUserRoleRequest request);
    Task<bool> DeleteUserAsync(int id);
}