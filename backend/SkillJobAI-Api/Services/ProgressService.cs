using Microsoft.EntityFrameworkCore;
using SkillJobAI.Api.Data;
using SkillJobAI.Api.Entities;
using SkillJobAI.Api.Models;
using SkillJobAI.Api.Models.Responses;

namespace SkillJobAI.Api.Services;

public class ProgressService : IProgressService
{
    private readonly AppDbContext _context;

    public ProgressService(AppDbContext context)
    {
        _context = context;
    }

    public async Task<(bool Success, string? ErrorMessage, LessonProgressResponse? Progress)> CompleteLessonAsync(
        int userId,
        LessonProgressRequest request)
    {
        var lessonExists = await _context.Lessons
            .AnyAsync(l => l.Id == request.LessonId);

        if (!lessonExists)
            return (false, "Lesson not found", null);

        var alreadyCompleted = await _context.LessonProgresses
            .AnyAsync(p =>
                p.UserId == userId &&
                p.LessonId == request.LessonId);

        if (alreadyCompleted)
            return (false, "Lesson already completed", null);

        var progress = new LessonProgress
        {
            UserId = userId,
            LessonId = request.LessonId,
            CompletedAt = DateTime.UtcNow
        };

        _context.LessonProgresses.Add(progress);
        await _context.SaveChangesAsync();

        return (true, null, new LessonProgressResponse
        {
            Id = progress.Id,
            LessonId = progress.LessonId,
            CompletedAt = progress.CompletedAt
        });
    }

    public async Task<List<LessonProgressResponse>> GetMyProgressAsync(int userId)
    {
        return await _context.LessonProgresses
            .Where(p => p.UserId == userId)
            .Select(p => new LessonProgressResponse
            {
                Id = p.Id,
                LessonId = p.LessonId,
                CompletedAt = p.CompletedAt
            })
            .ToListAsync();
    }
}