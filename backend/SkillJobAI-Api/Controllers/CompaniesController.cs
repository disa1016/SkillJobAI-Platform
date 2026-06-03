using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SkillJobAI.Api.Data;
using SkillJobAI.Api.Entities;

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

    // Alle Firmen abrufen
    [HttpGet]
    public async Task<IActionResult> GetCompanies()
    {
        var companies = await _context.Set<Company>().ToListAsync();

        return Ok(companies);
    }

    // Einzelne Firma abrufen
    [HttpGet("{id}")]
    public async Task<IActionResult> GetCompany(int id)
    {
        var company = await _context.Set<Company>()
            .Include(c => c.Jobs)
            .FirstOrDefaultAsync(c => c.Id == id);

        if (company == null)
            return NotFound();

        return Ok(company);
    }

    // Firma erstellen
    [Authorize]
    [HttpPost]
    public async Task<IActionResult> CreateCompany(Company company)
    {
        company.CreatedAt = DateTime.UtcNow;

        _context.Set<Company>().Add(company);

        await _context.SaveChangesAsync();

        return Ok(company);
    }

    // Firma bearbeiten
    [Authorize]
    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateCompany(int id, Company request)
    {
        var company = await _context.Set<Company>()
            .FirstOrDefaultAsync(c => c.Id == id);

        if (company == null)
            return NotFound();

        company.Name = request.Name;
        company.Description = request.Description;
        company.WebsiteUrl = request.WebsiteUrl;
        company.LogoUrl = request.LogoUrl;
        company.Location = request.Location;

        await _context.SaveChangesAsync();

        return Ok(company);
    }

    // Firma löschen
    [Authorize]
    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteCompany(int id)
    {
        var company = await _context.Set<Company>()
            .FirstOrDefaultAsync(c => c.Id == id);

        if (company == null)
            return NotFound();

        _context.Remove(company);

        await _context.SaveChangesAsync();

        return NoContent();
    }
}