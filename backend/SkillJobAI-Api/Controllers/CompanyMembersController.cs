using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SkillJobAI.Api.Models;
using SkillJobAI.Api.Services;

namespace SkillJobAI.Api.Controllers;

[ApiController]
[Route("api/company-members")]
[Authorize(Roles = "Admin")]
public class CompanyMembersController : ControllerBase
{
   private readonly ICompanyMemberService _companyMemberService;
   public CompanyMembersController(ICompanyMemberService companyMemberService)
    {
        _companyMemberService = companyMemberService;
    }

    [HttpGet]
    public async Task<IActionResult> GetCompanyMembers()
    {
        var members = await _companyMemberService.GetCompanyMembersAsync();
        return Ok(members);
    }

    [HttpPost]
    public async Task<IActionResult> AssignRecruiter(CompanyMemberRequest request)
    {
        var result = await _companyMemberService.AssignRecruiterAsync(request);

        if (result == null)
            return BadRequest();

        if (result.Message == "User not found.")
            return NotFound(result);

        if (result.Message == "Company not found.")
            return NotFound(result);

        if (result.Message == "Dieser Recruiter ist dieser Firma bereits zugewiesen.")
            return BadRequest(result);

        return Ok(result);
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> RemoveRecruiter(int id)
    {
        var deleted = await _companyMemberService.RemoveRecruiterAsync(id);

        if (!deleted)
            return NotFound(new { message = "Company member not found." });

        return NoContent();
    }
}