using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SkillJobAI.Api.Services;

namespace SkillJobAI.Api.Controllers;

[ApiController]
[Route("api/certificates")]
public class CertificatesController : ControllerBase
{
    private readonly ICertificateService _certificateService;

    public CertificatesController(ICertificateService certificateService)
    {
        _certificateService = certificateService;
    }

    private int? GetCurrentUserId()
    {
        var userIdValue = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        return int.TryParse(userIdValue, out var userId) ? userId : null;
    }

    [Authorize]
    [HttpGet("course/{courseId}")]
    public async Task<IActionResult> DownloadCertificate(int courseId)
    {
        var userId = GetCurrentUserId();

        if (userId == null)
            return Unauthorized();

        var result = await _certificateService.GenerateCourseCertificateAsync(
            userId.Value,
            courseId
        );

        if (!result.Success || result.PdfBytes == null || result.FileName == null)
            return BadRequest(new { message = result.ErrorMessage });

        return File(
            result.PdfBytes,
            "application/pdf",
            result.FileName
        );
    }
}