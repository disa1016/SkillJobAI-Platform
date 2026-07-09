using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SkillJobAI.Api.Models.Responses;
using SkillJobAI.Api.Services;

namespace SkillJobAI.Api.Controllers;

[ApiController]
[Route("api/recruiter/candidates")]
public class RecruiterCandidatesController : ControllerBase
{
    private readonly IRecruiterCandidateService _recruiterCandidateService;

    public RecruiterCandidatesController(IRecruiterCandidateService recruiterCandidateService)
    {
        _recruiterCandidateService = recruiterCandidateService;
    }

    [Authorize(Roles = "Recruiter,Admin")]
    [HttpGet]
    public async Task<IActionResult> GetCandidates([FromQuery] string? skill)
    {
        var candidates = await _recruiterCandidateService.GetCandidatesAsync(skill);
        return Ok(candidates);
    }

    [Authorize(Roles = "Recruiter,Admin")]
    [HttpGet("{id}")]
    public async Task<IActionResult> GetCandidate(int id)
    {
        var candidate = await _recruiterCandidateService.GetCandidateAsync(id);

        if (candidate == null)
        {
            return NotFound(new MessageResponse
            {
                Message = "Candidate not found."
            });
        }

        return Ok(candidate);
    }
}