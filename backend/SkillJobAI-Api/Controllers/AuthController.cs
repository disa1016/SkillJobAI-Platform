using Microsoft.AspNetCore.Mvc;
using SkillJobAI.Api.Models;
using SkillJobAI.Api.Models.Responses;
using SkillJobAI.Api.Services;
using Microsoft.AspNetCore.RateLimiting;

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
    [EnableRateLimiting("auth")]
    public async Task<IActionResult> Register(
        RegisterRequest request)
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
    [EnableRateLimiting("auth")]
    public async Task<IActionResult> Login(
        LoginRequest request)
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

    [HttpPost("refresh")]
    public async Task<IActionResult> RefreshToken(
        RefreshTokenRequest request)
    {
        var result = await _authService.RefreshTokenAsync(request);

        if (result == null)
        {
            return Unauthorized(new MessageResponse
            {
                Message = "Der Refresh Token ist ungültig oder abgelaufen."
            });
        }

        return Ok(result);
    }

    [HttpPost("logout")]
    public async Task<IActionResult> Logout(
        LogoutRequest request)
    {
        var success = await _authService.LogoutAsync(request);

        if (!success)
        {
            return BadRequest(new MessageResponse
            {
                Message = "Der Refresh Token ist ungültig oder bereits widerrufen."
            });
        }

        return Ok(new MessageResponse
        {
            Message = "Logout erfolgreich."
        });
    }

    [HttpPost("forgot-password")]
    [EnableRateLimiting("auth")]
    public async Task<IActionResult> ForgotPassword(
        ForgotPasswordRequest request)
    {
        var result = await _authService.ForgotPasswordAsync(request);

        return Ok(result);
    }
    [HttpGet("test-error")]
    public IActionResult TestError()
    {
        throw new InvalidOperationException(
            "Absichtlicher Testfehler für die GlobalExceptionMiddleware.");
    }

    [HttpPost("reset-password")]
    public async Task<IActionResult> ResetPassword(
        ResetPasswordRequest request)
    {
        var result = await _authService.ResetPasswordAsync(request);

        if (!result.Success)
        {
            return BadRequest(result);
        }

        return Ok(result);
    }
}