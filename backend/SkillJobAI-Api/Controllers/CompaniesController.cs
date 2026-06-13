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