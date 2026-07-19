using Microsoft.EntityFrameworkCore;
using SkillJobAI.Api.Data;
using SkillJobAI.Api.Entities;
using SkillJobAI.Api.Models;
using SkillJobAI.Api.Models.Responses;

namespace SkillJobAI.Api.Services;

public class CompanyService : ICompanyService
{
    private readonly AppDbContext _context;
    private const long MaxCompanyLogoSize = 10 * 1024 * 1024;

    public CompanyService(AppDbContext context)
    {
        _context = context;
    }

    public async Task<PagedResponse<CompanyResponse>> GetCompaniesAsync(
        int page,
        int pageSize,
        string? search)
    {
        if (page < 1)
            page = 1;

        if (pageSize < 1)
            pageSize = 10;

        if (pageSize > 50)
            pageSize = 50;

        var query = _context.Companies.AsQueryable();

        if (!string.IsNullOrWhiteSpace(search))
        {
            var searchTerm = search.ToLower();

            query = query.Where(c =>
                c.Name.ToLower().Contains(searchTerm) ||
                (c.Description != null && c.Description.ToLower().Contains(searchTerm)) ||
                (c.Location != null && c.Location.ToLower().Contains(searchTerm)) ||
                (c.WebsiteUrl != null && c.WebsiteUrl.ToLower().Contains(searchTerm)));
        }

        var totalItems = await query.CountAsync();

        var companies = await query
            .OrderBy(c => c.Name)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(c => new CompanyResponse
            {
                Id = c.Id,
                Name = c.Name,
                Description = c.Description ?? "",
                WebsiteUrl = c.WebsiteUrl ?? "",
                LogoUrl = c.LogoUrl ?? "",
                Location = c.Location ?? "",
                CreatedAt = c.CreatedAt,
                TotalJobs = c.Jobs.Count
            })
            .ToListAsync();

        return new PagedResponse<CompanyResponse>
        {
            Items = companies,
            Page = page,
            PageSize = pageSize,
            TotalItems = totalItems,
            TotalPages = (int)Math.Ceiling(totalItems / (double)pageSize)
        };
    }

    public async Task<CompanyResponse?> GetCompanyByIdAsync(int id)
    {
        return await _context.Companies
            .Where(c => c.Id == id)
            .Select(c => new CompanyResponse
            {
                Id = c.Id,
                Name = c.Name,
                Description = c.Description ?? "",
                WebsiteUrl = c.WebsiteUrl ?? "",
                LogoUrl = c.LogoUrl ?? "",
                Location = c.Location ?? "",
                CreatedAt = c.CreatedAt,
                TotalJobs = c.Jobs.Count,
                Jobs = c.Jobs.Select(j => new CompanyJobResponse
                {
                    Id = j.Id,
                    Title = j.Title,
                    Location = j.Location ?? "",
                    Salary = j.Salary ?? ""
                }).ToList()
            })
            .FirstOrDefaultAsync();
    }

    public async Task<CompanyResponse> CreateCompanyAsync(CompanyRequest request)
    {
        var company = new Company
        {
            Name = request.Name,
            Description = request.Description,
            WebsiteUrl = request.WebsiteUrl,
            LogoUrl = request.LogoUrl,
            Location = request.Location,
            CreatedAt = DateTime.UtcNow
        };

        _context.Companies.Add(company);
        await _context.SaveChangesAsync();

        return new CompanyResponse
        {
            Id = company.Id,
            Name = company.Name,
            Description = company.Description ?? "",
            WebsiteUrl = company.WebsiteUrl ?? "",
            LogoUrl = company.LogoUrl ?? "",
            Location = company.Location ?? "",
            CreatedAt = company.CreatedAt,
            TotalJobs = company.Jobs?.Count ?? 0
        };
    }

    public async Task<CompanyResponse?> UpdateCompanyAsync(int id, CompanyRequest request)
    {
        var company = await _context.Companies.FindAsync(id);

        if (company == null)
            return null;

        company.Name = request.Name;
        company.Description = request.Description;
        company.WebsiteUrl = request.WebsiteUrl;
        company.LogoUrl = request.LogoUrl;
        company.Location = request.Location;

        await _context.SaveChangesAsync();

        var totalJobs = await _context.Jobs
     .CountAsync(j => j.CompanyId == company.Id);

        return new CompanyResponse
        {
            Id = company.Id,
            Name = company.Name,
            Description = company.Description ?? "",
            WebsiteUrl = company.WebsiteUrl ?? "",
            LogoUrl = company.LogoUrl ?? "",
            Location = company.Location ?? "",
            CreatedAt = company.CreatedAt,
            TotalJobs = totalJobs
        };
    }

    public async Task<(bool Success, string? ErrorMessage, string? LogoUrl)> UploadCompanyLogoAsync(
        int id,
        IFormFile file)
    {
        var company = await _context.Companies.FindAsync(id);

        if (company == null)
            return (false, "Company not found.", null);

        if (file == null || file.Length == 0)
            return (false, "No file uploaded.", null);
        if (file.Length > MaxCompanyLogoSize)
        {
            return (
                false,
                "Company logo must be smaller than 10 MB.",
                null
            );
        }

        var allowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".webp" };
        var extension = Path.GetExtension(file.FileName).ToLower();

        if (!allowedExtensions.Contains(extension))
            return (false, "Only JPG, PNG and WEBP files are allowed.", null);

        var uploadsFolder = Path.Combine(
            Directory.GetCurrentDirectory(),
            "wwwroot",
            "uploads",
            "companies"
        );

        if (!Directory.Exists(uploadsFolder))
            Directory.CreateDirectory(uploadsFolder);

        var fileName = $"company-{id}-{Guid.NewGuid()}{extension}";
        var filePath = Path.Combine(uploadsFolder, fileName);

        await using var stream = new FileStream(filePath, FileMode.Create);
        await file.CopyToAsync(stream);

        company.LogoUrl = $"/uploads/companies/{fileName}";

        await _context.SaveChangesAsync();

        return (true, null, company.LogoUrl);
    }

    public async Task<bool> DeleteCompanyAsync(int id)
    {
        var company = await _context.Companies.FindAsync(id);

        if (company == null)
            return false;

        _context.Companies.Remove(company);
        await _context.SaveChangesAsync();

        return true;
    }
}