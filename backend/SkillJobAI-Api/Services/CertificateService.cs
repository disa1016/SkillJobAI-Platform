using Microsoft.EntityFrameworkCore;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using SkillJobAI.Api.Data;

namespace SkillJobAI.Api.Services;

public class CertificateService : ICertificateService
{
    private readonly AppDbContext _context;

    public CertificateService(AppDbContext context)
    {
        _context = context;
    }

    public async Task<(bool Success, string? ErrorMessage, byte[]? PdfBytes, string? FileName)>
        GenerateCourseCertificateAsync(int userId, int courseId)
    {
        var user = await _context.Users.FindAsync(userId);

        if (user == null)
            return (false, "User not found.", null, null);

        var course = await _context.Courses
            .Include(c => c.Lessons)
            .FirstOrDefaultAsync(c => c.Id == courseId);

        if (course == null)
            return (false, "Course not found", null, null);

        var totalLessons = course.Lessons.Count;

        if (totalLessons == 0)
            return (false, "Dieser Kurs hat noch keine Lektionen.", null, null);

        var lessonIds = course.Lessons.Select(l => l.Id).ToList();

        var completedLessons = await _context.LessonProgresses
            .CountAsync(p =>
                p.UserId == userId &&
                lessonIds.Contains(p.LessonId));

        if (completedLessons < totalLessons)
        {
            return (
                false,
                $"Du musst zuerst alle Lektionen abschließen. ({completedLessons}/{totalLessons})",
                null,
                null
            );
        }

        QuestPDF.Settings.License = LicenseType.Community;

        var pdfBytes = Document.Create(container =>
        {
            container.Page(page =>
            {
                page.Size(PageSizes.A4.Landscape());
                page.Margin(50);
                page.PageColor(Colors.White);
                page.DefaultTextStyle(x => x.FontSize(20));

                page.Content()
                    .Border(3)
                    .BorderColor(Colors.Blue.Medium)
                    .Padding(40)
                    .Column(column =>
                    {
                        column.Spacing(25);

                        column.Item().AlignCenter().Text("Certificate of Completion")
                            .FontSize(42)
                            .Bold()
                            .FontColor(Colors.Blue.Medium);

                        column.Item().AlignCenter().Text("This certifies that")
                            .FontSize(22);

                        column.Item().AlignCenter().Text(user.FullName)
                            .FontSize(34)
                            .Bold();

                        column.Item().AlignCenter().Text("has successfully completed the course")
                            .FontSize(22);

                        column.Item().AlignCenter().Text(course.Title)
                            .FontSize(30)
                            .Bold()
                            .FontColor(Colors.Green.Medium);

                        column.Item().AlignCenter().Text($"Completed on {DateTime.UtcNow:dd.MM.yyyy}")
                            .FontSize(18);

                        column.Item().PaddingTop(30).AlignCenter().Text("SkillJob AI")
                            .FontSize(24)
                            .Bold();
                    });
            });
        }).GeneratePdf();

        return (
            true,
            null,
            pdfBytes,
            $"certificate-{course.Title}.pdf"
        );
    }
}