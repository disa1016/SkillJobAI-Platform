using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SkillJobAI.Api.Data;
using SkillJobAI.Api.Entities;
using SkillJobAI.Api.Models;
using SkillJobAI.Api.Services;

namespace SkillJobAI.Api.Controllers;

[ApiController]
[Route("api/auth")]
public class AuthController : ControllerBase
{
    private readonly AppDbContext _context;
    private readonly JwtService _jwtService;
    private readonly PasswordService _passwordService;

    public AuthController(
        AppDbContext context,
        JwtService jwtService,
        PasswordService passwordService)
    {
        _context = context;
        _jwtService = jwtService;
        _passwordService = passwordService;
    }

    [HttpPost("register")]
    public async Task<IActionResult> Register(RegisterRequest request)
    {
        var emailExists = await _context.Users
            .AnyAsync(u => u.Email == request.Email);

        if (emailExists)
        {
            return BadRequest(new
            {
                message = "Diese E-Mail ist bereits registriert."
            });
        }

        var user = new AppUser
        {
            FullName = request.FullName,
            Email = request.Email,
            PasswordHash = _passwordService.HashPassword(request.Password),
            Role = "Student",
            CreatedAt = DateTime.UtcNow
        };

        _context.Users.Add(user);
        await _context.SaveChangesAsync();

        var token = _jwtService.GenerateToken(user);

        return Ok(new
        {
            message = "User registered successfully",
            token,
            user = new
            {
                id = user.Id,
                fullName = user.FullName,
                email = user.Email,
                role = user.Role
            }
        });
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login(LoginRequest request)
    {
        var user = await _context.Users
            .FirstOrDefaultAsync(u => u.Email == request.Email);

        if (user == null)
        {
            return Unauthorized(new
            {
                message = "E-Mail oder Passwort ist falsch."
            });
        }

        var passwordIsValid = _passwordService.VerifyPassword(
            request.Password,
            user.PasswordHash);

        if (!passwordIsValid)
        {
            return Unauthorized(new
            {
                message = "E-Mail oder Passwort ist falsch."
            });
        }

        var token = _jwtService.GenerateToken(user);

        return Ok(new
        {
            message = "Login successful",
            token,
            user = new
            {
                id = user.Id,
                fullName = user.FullName,
                email = user.Email,
                role = user.Role
            }
        });
    }

    [HttpPost("forgot-password")]
    public async Task<IActionResult> ForgotPassword(ForgotPasswordRequest request)
    {
        var user = await _context.Users
            .FirstOrDefaultAsync(u => u.Email == request.Email);

        if (user == null)
        {
            return NotFound(new
            {
                message = "Benutzer mit dieser E-Mail wurde nicht gefunden."
            });
        }

        user.PasswordHash = _passwordService.HashPassword(request.NewPassword);

        await _context.SaveChangesAsync();

        return Ok(new
        {
            message = "Passwort wurde erfolgreich geändert."
        });
    }
}