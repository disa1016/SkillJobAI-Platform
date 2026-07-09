using SkillJobAI.Api.Models;
using SkillJobAI.Api.Models.Responses;

namespace SkillJobAI.Api.Services;

public interface ICompanyMemberService
{
    Task<List<CompanyMemberResponse>> GetCompanyMembersAsync();
    Task<CompanyMemberActionResponse?> AssignRecruiterAsync(CompanyMemberRequest request);
    Task<bool> RemoveRecruiterAsync(int id);
}
