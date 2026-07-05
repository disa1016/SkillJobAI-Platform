using Microsoft.EntityFrameworkCore;
using SkillJobAI.Api.Data;
using SkillJobAI.Api.Entities;
using SkillJobAI.Api.Models;
using SkillJobAI.Api.Models.Responses;

namespace SkillJobAI.Api.Services;

public class EnrollmentService : IEnrollmentService
{
    private readonly AppDbContext _context;

    public EnrollmentService(AppDbContext context)
    {
        _context = context;
    }

    public async Task<(bool Success, string? ErrorMessage, EnrollmentResponse? Enrollment)> EnrollAsync(
        int userId,
        EnrollmentRequest request)
    {
        var courseExists = await _context.Courses.AnyAsync(c => c.Id == request.CourseId);

        if (!courseExists)
            return (false, "Course not found", null);

        var alreadyEnrolled = await _context.Enrollments
            .AnyAsync(e => e.UserId == userId && e.CourseId == request.CourseId);

        if (alreadyEnrolled)
            return (false, "Du bist bereits in diesem Kurs eingeschrieben.", null);

        var enrollment = new Enrollment
        {
            UserId = userId,
            CourseId = request.CourseId,
            EnrolledAt = DateTime.UtcNow,
            IsCompleted = false
        };

        _context.Enrollments.Add(enrollment);
        await _context.SaveChangesAsync();

        return (true, null, new EnrollmentResponse
        {
            Id = enrollment.Id,
            CourseId = enrollment.CourseId,
            EnrolledAt = enrollment.EnrolledAt,
            IsCompleted = enrollment.IsCompleted
        });
    }

    public async Task<List<EnrollmentResponse>> GetMyEnrollmentsAsync(int userId)
    {
        return await _context.Enrollments
            .Where(e => e.UserId == userId)
            .Select(e => new EnrollmentResponse
            {
                Id = e.Id,
                CourseId = e.CourseId,
                EnrolledAt = e.EnrolledAt,
                IsCompleted = e.IsCompleted,
                Course = _context.Courses
                    .Where(c => c.Id == e.CourseId)
                    .Select(c => new EnrollmentCourseResponse
                    {
                        Id = c.Id,
                        Title = c.Title,
                        Description = c.Description,
                        Category = c.Category,
                        Level = c.Level,
                        Instructor = c.Instructor
                    })
                    .FirstOrDefault()
            })
            .ToListAsync();
    }
}