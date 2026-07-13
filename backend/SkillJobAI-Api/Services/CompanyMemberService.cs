using Microsoft.EntityFrameworkCore;
using SkillJobAI.Api.Data;
using SkillJobAI.Api.Entities;
using SkillJobAI.Api.Models;
using SkillJobAI.Api.Models.Responses;

namespace SkillJobAI.Api.Services;

public class CompanyMemberService : ICompanyMemberService
{
    private readonly AppDbContext _context;

    public CompanyMemberService(AppDbContext context)
    {
        _context = context;
    }

    public async Task<List<CompanyMemberResponse>> GetCompanyMembersAsync()
    {
        return await _context.CompanyMembers
            .Include(cm => cm.User)
            .Include(cm => cm.Company)
            .OrderBy(cm => cm.Company.Name)
            .Select(cm => new CompanyMemberResponse
            {
                Id = cm.Id,
                UserId = cm.UserId,
                CompanyId = cm.CompanyId,
                Role = cm.Role,
                JoinedAt = cm.JoinedAt,
                Recruiter = new RecruiterInfoResponse
                {
                    Id = cm.User.Id,
                    FullName = cm.User.FullName,
                    Email = cm.User.Email
                },
                Company = new CompanyInfoResponse
                {
                    Id = cm.Company.Id,
                    Name = cm.Company.Name,
                    Location = cm.Company.Location
                }
            })
            .ToListAsync();
    }

    public async Task<CompanyMemberActionResponse?> AssignRecruiterAsync(CompanyMemberRequest request)
    {
        var user = await _context.Users.FindAsync(request.UserId);

        if (user == null)
        {
            return new CompanyMemberActionResponse
            {
                Message = "User not found."
            };
        }

        var company = await _context.Companies.FindAsync(request.CompanyId);

        if (company == null)
        {
            return new CompanyMemberActionResponse
            {
                Message = "Company not found."
            };
        }

        var exists = await _context.CompanyMembers
            .AnyAsync(cm =>
                cm.UserId == request.UserId &&
                cm.CompanyId == request.CompanyId);

        if (exists)
        {
            return new CompanyMemberActionResponse
            {
                Message = "Dieser Recruiter ist dieser Firma bereits zugewiesen."
            };
        }

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

        return new CompanyMemberActionResponse
        {
            Message = "Recruiter wurde Firma zugewiesen.",
            Id = member.Id,
            UserId = member.UserId,
            CompanyId = member.CompanyId,
            Role = member.Role,
            JoinedAt = member.JoinedAt
        };
    }

    public async Task<bool> RemoveRecruiterAsync(int id)
    {
        var member = await _context.CompanyMembers.FindAsync(id);

        if (member == null)
            return false;

        _context.CompanyMembers.Remove(member);
        await _context.SaveChangesAsync();

        return true;
    }
}