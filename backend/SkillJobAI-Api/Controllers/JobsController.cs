using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SkillJobAI.Api.Data;
using SkillJobAI.Api.Entities;

namespace SkillJobAI.Api.Controllers;

[ApiController]
[Route("api/jobs")]
public class JobsController : ControllerBase
{
    private readonly AppDbContext _context;

    public JobsController(AppDbContext context)
    {
        _context = context;
    }

    // Alle Jobs abrufen
    [HttpGet]
    public async Task<IActionResult> GetJobs()
    {
        var jobs = await _context.Jobs
            .Include(j => j.Company)
            .Select(j => new
            {
                id = j.Id,
                title = j.Title,
                description = j.Description,
                location = j.Location,
                salary = j.Salary,
                createdAt = j.CreatedAt,
                companyId = j.CompanyId,
                companyName = j.Company != null ? j.Company.Name : null,
                company = j.Company == null ? null : new
                {
                    id = j.Company.Id,
                    name = j.Company.Name,
                    location = j.Company.Location
                }
            })
            .ToListAsync();

        return Ok(jobs);
    }

    // Einzelnen Job abrufen
    [HttpGet("{id}")]
    public async Task<IActionResult> GetJob(int id)
    {
        var job = await _context.Jobs
            .Include(j => j.Company)
            .Where(j => j.Id == id)
            .Select(j => new
            {
                id = j.Id,
                title = j.Title,
                description = j.Description,
                location = j.Location,
                salary = j.Salary,
                createdAt = j.CreatedAt,
                companyId = j.CompanyId,
                companyName = j.Company != null ? j.Company.Name : null,
                company = j.Company == null ? null : new
                {
                    id = j.Company.Id,
                    name = j.Company.Name,
                    description = j.Company.Description,
                    websiteUrl = j.Company.WebsiteUrl,
                    logoUrl = j.Company.LogoUrl,
                    location = j.Company.Location
                }
            })
            .FirstOrDefaultAsync();

        if (job == null)
            return NotFound();

        return Ok(job);
    }

    // Job erstellen
    [Authorize]
    [HttpPost]
    public async Task<IActionResult> CreateJob(Job job)
    {
        var companyExists = await _context.Companies
            .AnyAsync(c => c.Id == job.CompanyId);

        if (!companyExists)
        {
            return BadRequest(new
            {
                message = "Company not found."
            });
        }

        job.CreatedAt = DateTime.UtcNow;

        _context.Jobs.Add(job);
        await _context.SaveChangesAsync();

        return Ok(new
        {
            id = job.Id,
            title = job.Title,
            description = job.Description,
            location = job.Location,
            salary = job.Salary,
            createdAt = job.CreatedAt,
            companyId = job.CompanyId
        });
    }

    [Authorize]
    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateJob(int id, Job request)
    {
        var job = await _context.Jobs.FindAsync(id);

        if (job == null)
            return NotFound(new { message = "Job not found." });

        var companyExists = await _context.Companies
            .AnyAsync(c => c.Id == request.CompanyId);

        if (!companyExists)
            return BadRequest(new { message = "Company not found." });

        job.Title = request.Title;
        job.Description = request.Description;
        job.Location = request.Location;
        job.Salary = request.Salary;
        job.CompanyId = request.CompanyId;

        await _context.SaveChangesAsync();

        return Ok(job);
    }

    [Authorize]
    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteJob(int id)
    {
        var job = await _context.Jobs.FindAsync(id);

        if (job == null)
            return NotFound(new { message = "Job not found." });

        _context.Jobs.Remove(job);
        await _context.SaveChangesAsync();

        return NoContent();

    }
}