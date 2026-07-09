using Microsoft.AspNetCore.Mvc;
using SkillJobAI.Api.Models;
using SkillJobAI.Api.Models.Responses;
using SkillJobAI.Api.Services;

namespace SkillJobAI.Api.Controllers;

[ApiController]
[Route("api/auth")]
public class AuthController : ControllerBase
{
    private readonly IAuthService _authService;

    public AuthController(IAuthService authService)
    {
        _authService = authService;
    }

    [HttpPost("register")]
    public async Task<IActionResult> Register(RegisterRequest request)
    {
        var result = await _authService.RegisterAsync(request);

        if (result == null)
        {
            return BadRequest(new MessageResponse
            {
                Message = "Diese E-Mail ist bereits registriert."
            });
        }

        return Ok(result);
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login(LoginRequest request)
    {
        var result = await _authService.LoginAsync(request);

        if (result == null)
        {
            return Unauthorized(new MessageResponse
            {
                Message = "E-Mail oder Passwort ist falsch."
            });
        }

        return Ok(result);
    }

    [HttpPost("forgot-password")]
    public async Task<IActionResult> ForgotPassword(ForgotPasswordRequest request)
    {
        var result = await _authService.ForgotPasswordAsync(request);

        if (result == null)
        {
            return NotFound(new MessageResponse
            {
                Message = "Benutzer mit dieser E-Mail wurde nicht gefunden."
            });
        }

        return Ok(result);
    }
}