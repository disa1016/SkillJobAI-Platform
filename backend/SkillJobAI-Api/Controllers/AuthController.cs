using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using SkillJobAI.Api.Models;
using SkillJobAI.Api.Models.Responses;
using SkillJobAI.Api.Services;

namespace SkillJobAI.Api.Controllers;

[ApiController]
[Route("api/auth")]
public class AuthController : ControllerBase
{
    private const string RefreshTokenCookieName =
        "skilljobai_refresh_token";

    private readonly IAuthService _authService;
    private readonly IWebHostEnvironment _environment;

    public AuthController(
        IAuthService authService,
        IWebHostEnvironment environment)
    {
        _authService = authService;
        _environment = environment;
    }

    [HttpPost("register")]
    [EnableRateLimiting("auth")]
    public async Task<IActionResult> Register(
        RegisterRequest request)
    {
        var result =
            await _authService.RegisterAsync(request);

        if (result == null)
        {
            return BadRequest(new MessageResponse
            {
                Message =
                    "Diese E-Mail ist bereits registriert."
            });
        }

        SetRefreshTokenCookie(result.RefreshToken);

        // Der Refresh Token darf nicht zusätzlich
        // im JSON-Body an JavaScript zurückgegeben werden.
        result.RefreshToken = string.Empty;

        return Ok(result);
    }

    [HttpPost("login")]
    [EnableRateLimiting("auth")]
    public async Task<IActionResult> Login(
        LoginRequest request)
    {
        var result =
            await _authService.LoginAsync(request);

        if (result == null)
        {
            return Unauthorized(new MessageResponse
            {
                Message =
                    "E-Mail oder Passwort ist falsch."
            });
        }

        SetRefreshTokenCookie(result.RefreshToken);

        // Der Refresh Token wird ausschließlich
        // als HttpOnly-Cookie übertragen.
        result.RefreshToken = string.Empty;

        return Ok(result);
    }

    [HttpPost("refresh")]
    public async Task<IActionResult> RefreshToken()
    {
        var rawRefreshToken =
            Request.Cookies[RefreshTokenCookieName];

        if (string.IsNullOrWhiteSpace(rawRefreshToken))
        {
            DeleteRefreshTokenCookie();

            return Unauthorized(new MessageResponse
            {
                Message =
                    "Es ist kein gültiger Refresh Token vorhanden."
            });
        }

        var result =
            await _authService.RefreshTokenAsync(
                new RefreshTokenRequest
                {
                    RefreshToken = rawRefreshToken
                });

        if (result == null)
        {
            DeleteRefreshTokenCookie();

            return Unauthorized(new MessageResponse
            {
                Message =
                    "Der Refresh Token ist ungültig oder abgelaufen."
            });
        }

        // Refresh-Token-Rotation:
        // Der bisherige Cookie wird durch den neuen Token ersetzt.
        SetRefreshTokenCookie(result.RefreshToken);

        result.RefreshToken = string.Empty;

        return Ok(result);
    }

    [HttpPost("logout")]
    public async Task<IActionResult> Logout()
    {
        var rawRefreshToken =
            Request.Cookies[RefreshTokenCookieName];

        if (!string.IsNullOrWhiteSpace(rawRefreshToken))
        {
            await _authService.LogoutAsync(
                new LogoutRequest
                {
                    RefreshToken = rawRefreshToken
                });
        }

        /*
         * Logout ist absichtlich idempotent:
         * Selbst wenn der Token bereits abgelaufen,
         * unbekannt oder widerrufen ist, wird der Cookie
         * gelöscht und eine erfolgreiche Antwort geliefert.
         */
        DeleteRefreshTokenCookie();

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
        var result =
            await _authService.ForgotPasswordAsync(request);

        return Ok(result);
    }

    [HttpPost("reset-password")]
    public async Task<IActionResult> ResetPassword(
        ResetPasswordRequest request)
    {
        var result =
            await _authService.ResetPasswordAsync(request);

        if (!result.Success)
        {
            return BadRequest(result);
        }

        return Ok(result);
    }

    private void SetRefreshTokenCookie(
        string refreshToken)
    {
        Response.Cookies.Append(
            RefreshTokenCookieName,
            refreshToken,
            CreateRefreshTokenCookieOptions());
    }

    private void DeleteRefreshTokenCookie()
    {
        Response.Cookies.Delete(
            RefreshTokenCookieName,
            new CookieOptions
            {
                HttpOnly = true,

                // Lokal läuft das Backend möglicherweise über HTTP.
                // In Produktion muss Secure immer true sein.
                Secure = !_environment.IsDevelopment(),

                SameSite = SameSiteMode.Strict,
                Path = "/api/auth"
            });
    }

    private CookieOptions
        CreateRefreshTokenCookieOptions()
    {
        return new CookieOptions
        {
            HttpOnly = true,

            // Im lokalen Development ist HTTP erlaubt.
            // Außerhalb von Development nur HTTPS.
            Secure = !_environment.IsDevelopment(),

            SameSite = SameSiteMode.Strict,

            // Das Cookie wird nur an Auth-Endpunkte geschickt.
            Path = "/api/auth",

            IsEssential = true,

            MaxAge = TimeSpan.FromDays(14)
        };
    }
}