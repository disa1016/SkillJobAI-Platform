using Microsoft.EntityFrameworkCore;
using SkillJobAI.Api.Data;
using SkillJobAI.Api.Entities;
using SkillJobAI.Api.Models;
using SkillJobAI.Api.Models.Responses;

namespace SkillJobAI.Api.Services;

public class LessonService : ILessonService
{
    private readonly AppDbContext _context;

    public LessonService(AppDbContext context)
    {
        _context = context;
    }

    public async Task<List<LessonResponse>> GetLessonsAsync()
    {
        return await _context.Lessons
            .OrderBy(l => l.OrderNumber)
            .Select(l => new LessonResponse
            {
                Id = l.Id,
                CourseId = l.CourseId,
                Title = l.Title,
                Content = l.Content,
                VideoUrl = l.VideoUrl ?? "",
                OrderNumber = l.OrderNumber,
                CreatedAt = l.CreatedAt
            })
            .ToListAsync();
    }

    public async Task<LessonResponse?> GetLessonByIdAsync(int id)
    {
        return await _context.Lessons
            .Where(l => l.Id == id)
            .Select(l => new LessonResponse
            {
                Id = l.Id,
                CourseId = l.CourseId,
                Title = l.Title,
                Content = l.Content,
                VideoUrl = l.VideoUrl ?? "",
                OrderNumber = l.OrderNumber,
                CreatedAt = l.CreatedAt
            })
            .FirstOrDefaultAsync();
    }

    public async Task<LessonResponse?> CreateLessonAsync(LessonRequest request)
    {
        var courseExists = await _context.Courses
            .AnyAsync(c => c.Id == request.CourseId);

        if (!courseExists)
            return null;

        var lesson = new Lesson
        {
            Title = request.Title,
            Content = request.Content,
            VideoUrl = request.VideoUrl ?? "",
            OrderNumber = request.OrderNumber,
            CourseId = request.CourseId,
            CreatedAt = DateTime.UtcNow
        };

        _context.Lessons.Add(lesson);
        await _context.SaveChangesAsync();

        return new LessonResponse
        {
            Id = lesson.Id,
            CourseId = lesson.CourseId,
            Title = lesson.Title,
            Content = lesson.Content,
            VideoUrl = lesson.VideoUrl,
            OrderNumber = lesson.OrderNumber,
            CreatedAt = lesson.CreatedAt
        };
    }
}