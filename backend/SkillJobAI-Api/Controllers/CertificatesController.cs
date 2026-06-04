using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using SkillJobAI.Api.Data;

namespace SkillJobAI.Api.Controllers;

[ApiController]
[Route("api/certificates")]
public class CertificatesController : ControllerBase
{
    private readonly AppDbContext _context;

    public CertificatesController(AppDbContext context)
    {
        _context = context;
    }

    [Authorize]
    [HttpGet("course/{courseId}")]
    public async Task<IActionResult> DownloadCertificate(int courseId)
    {
        var userIdValue = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

        if (userIdValue == null)
            return Unauthorized();

        var userId = int.Parse(userIdValue);

        var user = await _context.Users.FindAsync(userId);

        if (user == null)
            return Unauthorized();

        var course = await _context.Courses
            .Include(c => c.Lessons)
            .FirstOrDefaultAsync(c => c.Id == courseId);

        if (course == null)
            return NotFound(new { message = "Course not found" });

        var totalLessons = course.Lessons.Count;

        if (totalLessons == 0)
            return BadRequest(new { message = "Dieser Kurs hat noch keine Lektionen." });

        var completedLessons = await _context.LessonProgresses
            .CountAsync(p =>
                p.UserId == userId &&
                course.Lessons.Select(l => l.Id).Contains(p.LessonId));

        if (completedLessons < totalLessons)
        {
            return BadRequest(new
            {
                message = "Du musst zuerst alle Lektionen abschließen.",
                completedLessons,
                totalLessons
            });
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

        return File(
            pdfBytes,
            "application/pdf",
            $"certificate-{course.Title}.pdf"
        );
    }
}