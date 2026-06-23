using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SkillJobAI.Api.Data;
using SkillJobAI.Api.Entities;
using SkillJobAI.Api.Models;

namespace SkillJobAI.Api.Controllers;

[ApiController]
[Route("api/company-members")]
[Authorize(Roles = "Admin")]
public class CompanyMembersController : ControllerBase
{
    private readonly AppDbContext _context;

    public CompanyMembersController(AppDbContext context)
    {
        _context = context;
    }

    [HttpGet]
    public async Task<IActionResult> GetCompanyMembers()
    {
        var members = await _context.CompanyMembers
            .Include(cm => cm.User)
            .Include(cm => cm.Company)
            .OrderBy(cm => cm.Company.Name)
            .Select(cm => new
            {
                id = cm.Id,
                userId = cm.UserId,
                companyId = cm.CompanyId,
                role = cm.Role,
                joinedAt = cm.JoinedAt,
                recruiter = new
                {
                    id = cm.User.Id,
                    fullName = cm.User.FullName,
                    email = cm.User.Email
                },
                company = new
                {
                    id = cm.Company.Id,
                    name = cm.Company.Name,
                    location = cm.Company.Location
                }
            })
            .ToListAsync();

        return Ok(members);
    }

    [HttpPost]
    public async Task<IActionResult> AssignRecruiter(CompanyMemberRequest request)
    {
        var user = await _context.Users.FindAsync(request.UserId);

        if (user == null)
            return NotFound(new { message = "User not found." });

        var company = await _context.Companies.FindAsync(request.CompanyId);

        if (company == null)
            return NotFound(new { message = "Company not found." });

        var exists = await _context.CompanyMembers
            .AnyAsync(cm =>
                cm.UserId == request.UserId &&
                cm.CompanyId == request.CompanyId);

        if (exists)
            return BadRequest(new { message = "Dieser Recruiter ist dieser Firma bereits zugewiesen." });

        user.Role = "Recruiter";

        var member = new CompanyMember
        {
            UserId = request.UserId,
            CompanyId = request.CompanyId,
            Role = request.Role,
            JoinedAt = DateTime.UtcNow
        };

        _context.CompanyMembers.Add(member);
        await _context.SaveChangesAsync();

        return Ok(new
        {
            message = "Recruiter wurde Firma zugewiesen.",
            member.Id,
            member.UserId,
            member.CompanyId,
            member.Role,
            member.JoinedAt
        });
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> RemoveRecruiter(int id)
    {
        var member = await _context.CompanyMembers.FindAsync(id);

        if (member == null)
            return NotFound(new { message = "Company member not found." });

        _context.CompanyMembers.Remove(member);
        await _context.SaveChangesAsync();

        return NoContent();
    }
}