using Microsoft.EntityFrameworkCore;
using SkillJobAI.Api.Data;
using SkillJobAI.Api.Models;
using SkillJobAI.Api.Models.Responses;

namespace SkillJobAI.Api.Services;

public class AdminService : IAdminService
{
    private readonly AppDbContext _context;

    public AdminService(AppDbContext context)
    {
        _context = context;
    }

    public async Task<AdminDashboardResponse> GetDashboardAsync()
    {
        var today = DateTime.UtcNow.Date;
        var tomorrow = today.AddDays(1);

        return new AdminDashboardResponse
        {
            TotalUsers = await _context.Users.CountAsync(),
            TotalCompanies = await _context.Companies.CountAsync(),
            TotalJobs = await _context.Jobs.CountAsync(),
            TotalApplications = await _context.Applications.CountAsync(),
            TotalCourses = await _context.Courses.CountAsync(),
            TotalSkills = await _context.Skills.CountAsync(),

            NewUsersToday = await _context.Users
                .CountAsync(u => u.CreatedAt >= today && u.CreatedAt < tomorrow),

            NewApplicationsToday = await _context.Applications
                .CountAsync(a => a.CreatedAt >= today && a.CreatedAt < tomorrow),

            TotalRecruiters = await _context.Users.CountAsync(u => u.Role == "Recruiter"),
            TotalAdmins = await _context.Users.CountAsync(u => u.Role == "Admin")
        };
    }

    public async Task<List<AdminUserResponse>> GetUsersAsync()
    {
        return await _context.Users
            .Select(u => new AdminUserResponse
            {
                Id = u.Id,
                FullName = u.FullName,
                Email = u.Email,
                Role = u.Role,
                CreatedAt = u.CreatedAt
            })
            .OrderBy(u => u.Id)
            .ToListAsync();
    }

    public async Task<AdminUserResponse?> UpdateUserRoleAsync(int id, UpdateUserRoleRequest request)
    {
        var allowedRoles = new[] { "Candidate", "Student", "Recruiter", "Admin" };

        if (!allowedRoles.Contains(request.Role))
            return null;

        var user = await _context.Users.FindAsync(id);

        if (user == null)
            return null;

        user.Role = request.Role;
        await _context.SaveChangesAsync();

        return new AdminUserResponse
        {
            Id = user.Id,
            FullName = user.FullName,
            Email = user.Email,
            Role = user.Role,
            CreatedAt = user.CreatedAt
        };
    }

    public async Task<bool> DeleteUserAsync(int id)
    {
        var user = await _context.Users.FindAsync(id);

        if (user == null)
            return false;

        _context.Users.Remove(user);
        await _context.SaveChangesAsync();

        return true;
    }
}