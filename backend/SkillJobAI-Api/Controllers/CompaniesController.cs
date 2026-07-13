using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SkillJobAI.Api.Models;
using SkillJobAI.Api.Services;

namespace SkillJobAI.Api.Controllers;

[ApiController]
[Route("api/companies")]
public class CompaniesController : ControllerBase
{
    private readonly ICompanyService _companyService;

    public CompaniesController(ICompanyService companyService)
    {
        _companyService = companyService;
    }

    [HttpGet]
    public async Task<IActionResult> GetCompanies(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 10,
        [FromQuery] string? search = null)
    {
        var response = await _companyService.GetCompaniesAsync(page, pageSize, search);
        return Ok(response);
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetCompany(int id)
    {
        var company = await _companyService.GetCompanyByIdAsync(id);

        if (company == null)
            return NotFound(new { message = "Company not found." });

        return Ok(company);
    }

    [Authorize(Roles = "Admin")]
    [HttpPost]
    public async Task<IActionResult> CreateCompany([FromBody] CompanyRequest request)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        var company = await _companyService.CreateCompanyAsync(request);

        return Ok(company);
    }

    [Authorize(Roles = "Admin")]
    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateCompany(
        int id,
        [FromBody] CompanyRequest request)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        var company = await _companyService.UpdateCompanyAsync(id, request);

        if (company == null)
            return NotFound(new { message = "Company not found." });

        return Ok(company);
    }

    [Authorize(Roles = "Admin")]
    [HttpPost("{id}/logo")]
    public async Task<IActionResult> UploadCompanyLogo(int id, IFormFile file)
    {
        var result = await _companyService.UploadCompanyLogoAsync(id, file);

        if (!result.Success)
            return BadRequest(new { message = result.ErrorMessage });

        return Ok(new
        {
            message = "Logo uploaded successfully.",
            logoUrl = result.LogoUrl
        });
    }

    [Authorize(Roles = "Admin")]
    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteCompany(int id)
    {
        var deleted = await _companyService.DeleteCompanyAsync(id);

        if (!deleted)
            return NotFound(new { message = "Company not found." });

        return NoContent();
    }
}