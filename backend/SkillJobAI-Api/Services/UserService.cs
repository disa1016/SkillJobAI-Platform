using Microsoft.EntityFrameworkCore;
using SkillJobAI.Api.Data;
using SkillJobAI.Api.Models;
using SkillJobAI.Api.Models.Responses;

namespace SkillJobAI.Api.Services;

public class UserService : IUserService
{
    private readonly AppDbContext _context;

    public UserService(AppDbContext context)
    {
        _context = context;
    }

    public async Task<UserResponse?> GetProfileAsync(int userId)
    {
        return await _context.Users
            .Where(u => u.Id == userId)
            .Select(u => new UserResponse
            {
                Id = u.Id,
                FullName = u.FullName,
                Email = u.Email,
                Role = u.Role,
                CvUrl = u.CvUrl ?? "",
                CreatedAt = u.CreatedAt
            })
            .FirstOrDefaultAsync();
    }

    public async Task<(bool Success, string? ErrorMessage, string? CvUrl)> UploadCvAsync(
        int userId,
        IFormFile file)
    {
        var user = await _context.Users.FindAsync(userId);

        if (user == null)
            return (false, "User not found.", null);

        if (file == null || file.Length == 0)
            return (false, "No file uploaded.", null);

        var extension = Path.GetExtension(file.FileName).ToLower();

        if (extension != ".pdf")
            return (false, "Only PDF files are allowed.", null);

        if (file.Length > 5 * 1024 * 1024)
            return (false, "PDF file must be smaller than 5MB.", null);

        var uploadsFolder = Path.Combine(
            Directory.GetCurrentDirectory(),
            "wwwroot",
            "uploads",
            "cv"
        );

        if (!Directory.Exists(uploadsFolder))
            Directory.CreateDirectory(uploadsFolder);

        var fileName = $"cv-user-{user.Id}-{Guid.NewGuid()}.pdf";
        var filePath = Path.Combine(uploadsFolder, fileName);

        await using var stream = new FileStream(filePath, FileMode.Create);
        await file.CopyToAsync(stream);

        user.CvUrl = $"/uploads/cv/{fileName}";

        await _context.SaveChangesAsync();

        return (true, null, user.CvUrl);
    }

    public async Task<bool> DeleteCvAsync(int userId)
    {
        var user = await _context.Users.FindAsync(userId);

        if (user == null)
            return false;

        if (!string.IsNullOrEmpty(user.CvUrl))
        {
            var relativePath = user.CvUrl
                .TrimStart('/')
                .Replace("/", Path.DirectorySeparatorChar.ToString());

            var filePath = Path.Combine(
                Directory.GetCurrentDirectory(),
                "wwwroot",
                relativePath
            );

            if (File.Exists(filePath))
                File.Delete(filePath);
        }

        user.CvUrl = null;

        await _context.SaveChangesAsync();

        return true;
    }

    public async Task<UserResponse?> UpdateUserRoleAsync(
        int id,
        UpdateUserRoleRequest request)
    {
        var user = await _context.Users.FindAsync(id);

        if (user == null)
            return null;

        user.Role = request.Role;

        await _context.SaveChangesAsync();

        return new UserResponse
        {
            Id = user.Id,
            FullName = user.FullName,
            Email = user.Email,
            Role = user.Role,
            CvUrl = user.CvUrl ?? "",
            CreatedAt = user.CreatedAt
        };
    }
}