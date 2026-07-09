using Microsoft.EntityFrameworkCore;
using SkillJobAI.Api.Data;
using SkillJobAI.Api.Entities;
using SkillJobAI.Api.Models.Responses;

namespace SkillJobAI.Api.Services;

public class UserSkillService : IUserSkillService
{
    private readonly AppDbContext _context;

    public UserSkillService(AppDbContext context)
    {
        _context = context;
    }

    public async Task<List<SkillResponse>> GetMySkillsAsync(int userId)
    {
        return await _context.UserSkills
            .Where(us => us.UserId == userId)
            .Include(us => us.Skill)
            .Select(us => new SkillResponse
            {
                Id = us.Skill.Id,
                Name = us.Skill.Name
            })
            .ToListAsync();
    }

    public async Task<(bool Success, string? ErrorMessage)> AddSkillToUserAsync(
        int userId,
        int skillId)
    {
        var skillExists = await _context.Skills
            .AnyAsync(s => s.Id == skillId);

        if (!skillExists)
            return (false, "Skill not found.");

        var alreadyExists = await _context.UserSkills
            .AnyAsync(us => us.UserId == userId && us.SkillId == skillId);

        if (alreadyExists)
            return (false, "Skill already added to your profile.");

        var userSkill = new UserSkill
        {
            UserId = userId,
            SkillId = skillId
        };

        _context.UserSkills.Add(userSkill);
        await _context.SaveChangesAsync();

        return (true, null);
    }

    public async Task<(bool Success, string? ErrorMessage)> RemoveSkillFromUserAsync(
        int userId,
        int skillId)
    {
        var userSkill = await _context.UserSkills
            .FirstOrDefaultAsync(us => us.UserId == userId && us.SkillId == skillId);

        if (userSkill == null)
            return (false, "Skill is not assigned to your profile.");

        _context.UserSkills.Remove(userSkill);
        await _context.SaveChangesAsync();

        return (true, null);
    }
}