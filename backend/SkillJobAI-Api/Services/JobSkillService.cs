using Microsoft.EntityFrameworkCore;
using SkillJobAI.Api.Data;
using SkillJobAI.Api.Entities;
using SkillJobAI.Api.Models.Responses;

namespace SkillJobAI.Api.Services;

public class JobSkillService : IJobSkillService
{
    private readonly AppDbContext _context;

    public JobSkillService(AppDbContext context)
    {
        _context = context;
    }

    public async Task<List<SkillResponse>?> GetJobSkillsAsync(int jobId)
    {
        var jobExists = await _context.Jobs.AnyAsync(j => j.Id == jobId);

        if (!jobExists)
            return null;

        return await _context.JobSkills
            .Where(js => js.JobId == jobId)
            .Include(js => js.Skill)
            .Select(js => new SkillResponse
            {
                Id = js.Skill.Id,
                Name = js.Skill.Name
            })
            .ToListAsync();
    }

    public async Task<(bool Success, string? ErrorMessage)> AddSkillToJobAsync(int jobId, int skillId)
    {
        var jobExists = await _context.Jobs.AnyAsync(j => j.Id == jobId);

        if (!jobExists)
            return (false, "Job not found.");

        var skillExists = await _context.Skills.AnyAsync(s => s.Id == skillId);

        if (!skillExists)
            return (false, "Skill not found.");

        var alreadyExists = await _context.JobSkills
            .AnyAsync(js => js.JobId == jobId && js.SkillId == skillId);

        if (alreadyExists)
            return (false, "Skill already added to this job.");

        var jobSkill = new JobSkill
        {
            JobId = jobId,
            SkillId = skillId
        };

        _context.JobSkills.Add(jobSkill);
        await _context.SaveChangesAsync();

        return (true, null);
    }

    public async Task<(bool Success, string? ErrorMessage)> RemoveSkillFromJobAsync(int jobId, int skillId)
    {
        var jobSkill = await _context.JobSkills
            .FirstOrDefaultAsync(js => js.JobId == jobId && js.SkillId == skillId);

        if (jobSkill == null)
            return (false, "Skill is not assigned to this job.");

        _context.JobSkills.Remove(jobSkill);
        await _context.SaveChangesAsync();

        return (true, null);
    }
}