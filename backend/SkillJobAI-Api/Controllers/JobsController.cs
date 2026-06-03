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
            .ToListAsync();

        return Ok(jobs);
    }

    // Einzelnen Job abrufen
    [HttpGet("{id}")]
    public async Task<IActionResult> GetJob(int id)
    {
        var job = await _context.Jobs
            .Include(j => j.Company)
            .FirstOrDefaultAsync(j => j.Id == id);

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

        return Ok(job);
    }
}