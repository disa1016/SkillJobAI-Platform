using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SkillJobAI.Api.Models;
using SkillJobAI.Api.Models.Responses;
using SkillJobAI.Api.Services;
using Microsoft.AspNetCore.RateLimiting;

namespace SkillJobAI.Api.Controllers;

[ApiController]
[Route("api/ai")]
public class AiController : ControllerBase
{
    private readonly IAiService _aiService;

    public AiController(IAiService aiService)
    {
        _aiService = aiService;
    }

    [Authorize]
    [HttpPost("analyze-cv")]
    [EnableRateLimiting("ai")]
    public IActionResult AnalyzeCv(AnalyzeCvRequest request)
    {
        var result = _aiService.AnalyzeCv(request);
        return Ok(result);
    }

    [Authorize]
    [HttpPost("analyze-cv-pdf")]
    public async Task<IActionResult> AnalyzeCvPdf(IFormFile file)
    {
        if (file == null || file.Length == 0)
        {
            return BadRequest(new MessageResponse
            {
                Message = "Bitte lade eine PDF-Datei hoch."
            });
        }

        if (!file.FileName.EndsWith(".pdf", StringComparison.OrdinalIgnoreCase))
        {
            return BadRequest(new MessageResponse
            {
                Message = "Nur PDF-Dateien sind erlaubt."
            });
        }

        var result = await _aiService.AnalyzeCvPdfAsync(file);

        if (result == null)
        {
            return BadRequest(new MessageResponse
            {
                Message = "Aus dieser PDF konnte kein Text gelesen werden."
            });
        }

        return Ok(result);
    }

    [Authorize]
    [HttpPost("job-match")]
    public IActionResult JobMatch(JobMatchRequest request)
    {
        var result = _aiService.JobMatch(request);
        return Ok(result);
    }

    [Authorize]
    [HttpPost("job-recommendations")]
    public async Task<IActionResult> JobRecommendations(JobRecommendationsRequest request)
    {
        var result = await _aiService.JobRecommendationsAsync(request);
        return Ok(result);
    }

    [Authorize]
    [HttpPost("generate-cover-letter")]
    public IActionResult GenerateCoverLetter(CoverLetterRequest request)
    {
        var result = _aiService.GenerateCoverLetter(request);
        return Ok(result);
    }
}