using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SkillJobAI.Api.Data;
using SkillJobAI.Api.Entities;
using SkillJobAI.Api.Models;

namespace SkillJobAI.Api.Controllers;

[ApiController]
[Route("api/companies")]
public class CompaniesController : ControllerBase
{
    private readonly AppDbContext _context;

    public CompaniesController(AppDbContext context)
    {
        _context = context;
    }

    [HttpGet]
    public async Task<IActionResult> GetCompanies()
    {
        var companies = await _context.Companies
            .Select(c => new
            {
                id = c.Id,
                name = c.Name,
                description = c.Description,
                websiteUrl = c.WebsiteUrl,
                logoUrl = c.LogoUrl,
                location = c.Location,
                createdAt = c.CreatedAt,
                totalJobs = c.Jobs.Count
            })
            .ToListAsync();

        return Ok(companies);
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetCompany(int id)
    {
        var company = await _context.Companies
            .Where(c => c.Id == id)
            .Select(c => new
            {
                id = c.Id,
                name = c.Name,
                description = c.Description,
                websiteUrl = c.WebsiteUrl,
                logoUrl = c.LogoUrl,
                location = c.Location,
                createdAt = c.CreatedAt,
                jobs = c.Jobs.Select(j => new
                {
                    id = j.Id,
                    title = j.Title,
                    location = j.Location,
                    salary = j.Salary
                })
            })
            .FirstOrDefaultAsync();

        if (company == null)
            return NotFound(new { message = "Company not found." });

        return Ok(company);
    }

    [Authorize(Roles = "Recruiter,Admin")]
    [HttpPost]
    public async Task<IActionResult> CreateCompany(CompanyRequest request)
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

        return Ok(new
        {
            id = company.Id,
            name = company.Name,
            description = company.Description,
            websiteUrl = company.WebsiteUrl,
            logoUrl = company.LogoUrl,
            location = company.Location,
            createdAt = company.CreatedAt
        });
    }

    [Authorize(Roles = "Recruiter,Admin")]
    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateCompany(int id, CompanyRequest request)
    {
        var company = await _context.Companies.FindAsync(id);

        if (company == null)
            return NotFound(new { message = "Company not found." });

        company.Name = request.Name;
        company.Description = request.Description;
        company.WebsiteUrl = request.WebsiteUrl;
        company.LogoUrl = request.LogoUrl;
        company.Location = request.Location;

        await _context.SaveChangesAsync();

        return Ok(new
        {
            id = company.Id,
            name = company.Name,
            description = company.Description,
            websiteUrl = company.WebsiteUrl,
            logoUrl = company.LogoUrl,
            location = company.Location,
            createdAt = company.CreatedAt
        });
    }

    [Authorize(Roles = "Recruiter,Admin")]
    [HttpPost("{id}/logo")]
    public async Task<IActionResult> UploadCompanyLogo(int id, IFormFile file)
    {
        var company = await _context.Companies.FindAsync(id);

        if (company == null)
            return NotFound(new { message = "Company not found." });

        if (file == null || file.Length == 0)
            return BadRequest(new { message = "No file uploaded." });

        var allowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".webp" };
        var extension = Path.GetExtension(file.FileName).ToLower();

        if (!allowedExtensions.Contains(extension))
        {
            return BadRequest(new
            {
                message = "Only JPG, PNG and WEBP files are allowed."
            });
        }

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

        using (var stream = new FileStream(filePath, FileMode.Create))
        {
            await file.CopyToAsync(stream);
        }

        company.LogoUrl = $"/uploads/companies/{fileName}";

        await _context.SaveChangesAsync();

        return Ok(new
        {
            message = "Logo uploaded successfully.",
            logoUrl = company.LogoUrl
        });
    }

    [Authorize(Roles = "Admin")]
    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteCompany(int id)
    {
        var company = await _context.Companies.FindAsync(id);

        if (company == null)
            return NotFound(new { message = "Company not found." });

        _context.Companies.Remove(company);
        await _context.SaveChangesAsync();

        return NoContent();
    }
}