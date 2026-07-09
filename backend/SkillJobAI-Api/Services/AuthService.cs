using Microsoft.EntityFrameworkCore;
using SkillJobAI.Api.Constants;
using SkillJobAI.Api.Data;
using SkillJobAI.Api.Entities;
using SkillJobAI.Api.Models;
using SkillJobAI.Api.Models.Responses;

namespace SkillJobAI.Api.Services;

public class AuthService : IAuthService
{
    private readonly AppDbContext _context;
    private readonly JwtService _jwtService;
    private readonly PasswordService _passwordService;

    public AuthService(
        AppDbContext context,
        JwtService jwtService,
        PasswordService passwordService)
    {
        _context = context;
        _jwtService = jwtService;
        _passwordService = passwordService;
    }

    public async Task<AuthResponse?> RegisterAsync(RegisterRequest request)
    {
        var emailExists = await _context.Users
            .AnyAsync(u => u.Email == request.Email);

        if (emailExists)
            return null;

        var user = new AppUser
        {
            FullName = request.FullName,
            Email = request.Email,
            PasswordHash = _passwordService.HashPassword(request.Password),
            Role = request.Role == AppRoles.Recruiter
                ? AppRoles.Recruiter
                : AppRoles.Candidate,
            CreatedAt = DateTime.UtcNow
        };

        _context.Users.Add(user);
        await _context.SaveChangesAsync();

        var token = _jwtService.GenerateToken(user);

        return new AuthResponse
        {
            Message = "User registered successfully",
            Token = token,
            User = new AuthUserResponse
            {
                Id = user.Id,
                FullName = user.FullName,
                Email = user.Email,
                Role = user.Role
            }
        };
    }

    public async Task<AuthResponse?> LoginAsync(LoginRequest request)
    {
        var user = await _context.Users
            .FirstOrDefaultAsync(u => u.Email == request.Email);

        if (user == null)
            return null;

        var passwordIsValid = _passwordService.VerifyPassword(
            request.Password,
            user.PasswordHash);

        if (!passwordIsValid)
            return null;

        var token = _jwtService.GenerateToken(user);

        return new AuthResponse
        {
            Message = "Login successful",
            Token = token,
            User = new AuthUserResponse
            {
                Id = user.Id,
                FullName = user.FullName,
                Email = user.Email,
                Role = user.Role
            }
        };
    }

    public async Task<MessageResponse?> ForgotPasswordAsync(ForgotPasswordRequest request)
    {
        var user = await _context.Users
            .FirstOrDefaultAsync(u => u.Email == request.Email);

        if (user == null)
            return null;

        user.PasswordHash = _passwordService.HashPassword(request.NewPassword);

        await _context.SaveChangesAsync();

        return new MessageResponse
        {
            Message = "Passwort wurde erfolgreich geändert."
        };
    }
}