using Microsoft.EntityFrameworkCore;
using SkillJobAI.Api.Data;
using SkillJobAI.Api.Entities;
using SkillJobAI.Api.Models;
using SkillJobAI.Api.Models.Responses;

namespace SkillJobAI.Api.Services;

public class CourseService : ICourseService
{
    private readonly AppDbContext _context;

    public CourseService(AppDbContext context)
    {
        _context = context;
    }

    public async Task<PagedResponse<CourseResponse>> GetCoursesAsync(
        int page,
        int pageSize,
        string? search)
    {
        if (page < 1) page = 1;
        if (pageSize < 1) pageSize = 10;
        if (pageSize > 50) pageSize = 50;

        var query = _context.Courses.AsQueryable();

        if (!string.IsNullOrWhiteSpace(search))
        {
            var searchTerm = search.ToLower();

            query = query.Where(c =>
                c.Title.ToLower().Contains(searchTerm) ||
                c.Description.ToLower().Contains(searchTerm) ||
                c.Category.ToLower().Contains(searchTerm) ||
                c.Level.ToLower().Contains(searchTerm) ||
                c.Instructor.ToLower().Contains(searchTerm));
        }

        var totalItems = await query.CountAsync();

        var courses = await query
            .OrderByDescending(c => c.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(c => new CourseResponse
            {
                Id = c.Id,
                Title = c.Title,
                Description = c.Description,
                Category = c.Category,
                Level = c.Level,
                Instructor = c.Instructor,
                CreatedAt = c.CreatedAt
            })
            .ToListAsync();

        return new PagedResponse<CourseResponse>
        {
            Items = courses,
            Page = page,
            PageSize = pageSize,
            TotalItems = totalItems,
            TotalPages = (int)Math.Ceiling(totalItems / (double)pageSize)
        };
    }

    public async Task<CourseResponse?> GetCourseByIdAsync(int id)
    {
        return await _context.Courses
            .Where(c => c.Id == id)
            .Select(c => new CourseResponse
            {
                Id = c.Id,
                Title = c.Title,
                Description = c.Description,
                Category = c.Category,
                Level = c.Level,
                Instructor = c.Instructor,
                CreatedAt = c.CreatedAt,
                Lessons = c.Lessons
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
                    .ToList()
            })
            .FirstOrDefaultAsync();
    }

    public async Task<CourseResponse> CreateCourseAsync(CourseRequest request)
    {
        var course = new Course
        {
            Title = request.Title,
            Description = request.Description,
            Category = request.Category,
            Level = request.Level,
            Instructor = request.Instructor,
            CreatedAt = DateTime.UtcNow
        };

        _context.Courses.Add(course);
        await _context.SaveChangesAsync();

        return new CourseResponse
        {
            Id = course.Id,
            Title = course.Title,
            Description = course.Description,
            Category = course.Category,
            Level = course.Level,
            Instructor = course.Instructor,
            CreatedAt = course.CreatedAt
        };
    }

    public async Task<bool> DeleteCourseAsync(int id)
    {
        var course = await _context.Courses.FindAsync(id);

        if (course == null)
            return false;

        _context.Courses.Remove(course);
        await _context.SaveChangesAsync();

        return true;
    }
}