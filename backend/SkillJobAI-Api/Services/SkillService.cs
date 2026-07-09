using Microsoft.EntityFrameworkCore;
using SkillJobAI.Api.Data;
using SkillJobAI.Api.Entities;
using SkillJobAI.Api.Models;
using SkillJobAI.Api.Models.Responses;

namespace SkillJobAI.Api.Services;

public class SkillService : ISkillService
{
    private readonly AppDbContext _context;

    public SkillService(AppDbContext context)
    {
        _context = context;
    }

    public async Task<List<SkillResponse>> GetSkillsAsync()
    {
        return await _context.Skills
            .Select(s => new SkillResponse
            {
                Id = s.Id,
                Name = s.Name
            })
            .ToListAsync();
    }

    public async Task<SkillResponse?> CreateSkillAsync(SkillRequest request)
    {
        var exists = await _context.Skills
            .AnyAsync(s => s.Name.ToLower() == request.Name.ToLower());

        if (exists)
            return null;

        var skill = new Skill
        {
            Name = request.Name
        };

        _context.Skills.Add(skill);
        await _context.SaveChangesAsync();

        return new SkillResponse
        {
            Id = skill.Id,
            Name = skill.Name
        };
    }
}