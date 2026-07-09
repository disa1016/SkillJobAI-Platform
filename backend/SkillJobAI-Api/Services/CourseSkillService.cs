using Microsoft.EntityFrameworkCore;
using SkillJobAI.Api.Data;
using SkillJobAI.Api.Entities;
using SkillJobAI.Api.Models.Responses;

namespace SkillJobAI.Api.Services;

public class CourseSkillService : ICourseSkillService
{
    private readonly AppDbContext _context;

    public CourseSkillService(AppDbContext context)
    {
        _context = context;
    }

    public async Task<List<CourseSkillResponse>?> GetCourseSkillsAsync(int courseId)
    {
        var courseExists = await _context.Courses.AnyAsync(c => c.Id == courseId);

        if (!courseExists)
            return null;

        return await _context.CourseSkills
            .Where(cs => cs.CourseId == courseId)
            .Include(cs => cs.Skill)
            .Select(cs => new CourseSkillResponse
            {
                Id = cs.Skill.Id,
                Name = cs.Skill.Name
            })
            .ToListAsync();
    }

    public async Task<CourseSkillActionResponse> AddSkillToCourseAsync(int courseId, int skillId)
    {
        var courseExists = await _context.Courses.AnyAsync(c => c.Id == courseId);

        if (!courseExists)
            return new CourseSkillActionResponse { Message = "Course not found.", CourseId = courseId, SkillId = skillId };

        var skillExists = await _context.Skills.AnyAsync(s => s.Id == skillId);

        if (!skillExists)
            return new CourseSkillActionResponse { Message = "Skill not found.", CourseId = courseId, SkillId = skillId };

        var alreadyExists = await _context.CourseSkills
            .AnyAsync(cs => cs.CourseId == courseId && cs.SkillId == skillId);

        if (alreadyExists)
            return new CourseSkillActionResponse { Message = "Skill already added to this course.", CourseId = courseId, SkillId = skillId };

        var courseSkill = new CourseSkill
        {
            CourseId = courseId,
            SkillId = skillId
        };

        _context.CourseSkills.Add(courseSkill);
        await _context.SaveChangesAsync();

        return new CourseSkillActionResponse
        {
            Message = "Skill added to course successfully.",
            CourseId = courseId,
            SkillId = skillId
        };
    }

    public async Task<CourseSkillActionResponse> RemoveSkillFromCourseAsync(int courseId, int skillId)
    {
        var courseSkill = await _context.CourseSkills
            .FirstOrDefaultAsync(cs => cs.CourseId == courseId && cs.SkillId == skillId);

        if (courseSkill == null)
            return new CourseSkillActionResponse { Message = "Skill is not assigned to this course.", CourseId = courseId, SkillId = skillId };

        _context.CourseSkills.Remove(courseSkill);
        await _context.SaveChangesAsync();

        return new CourseSkillActionResponse
        {
            Message = "Skill removed from course successfully.",
            CourseId = courseId,
            SkillId = skillId
        };
    }
}