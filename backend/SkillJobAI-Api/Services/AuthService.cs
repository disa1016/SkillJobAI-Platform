using System.Security.Cryptography;
using System.Text;
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
    private readonly IEmailService _emailService;
    private readonly IConfiguration _configuration;
    private readonly ILogger<AuthService> _logger;

    public AuthService(
        AppDbContext context,
        JwtService jwtService,
        PasswordService passwordService,
        IEmailService emailService,
        IConfiguration configuration,
        ILogger<AuthService> logger)
    {
        _context = context;
        _jwtService = jwtService;
        _passwordService = passwordService;
        _emailService = emailService;
        _configuration = configuration;
        _logger = logger;
    }

    public async Task<AuthResponse?> RegisterAsync(RegisterRequest request)
    {
        var normalizedEmail = NormalizeEmail(request.Email);

        var emailExists = await _context.Users
            .AnyAsync(user => user.Email.ToLower() == normalizedEmail);

        if (emailExists)
            return null;

        var user = new AppUser
        {
            FullName = request.FullName.Trim(),
            Email = normalizedEmail,
            PasswordHash = _passwordService.HashPassword(request.Password),

            // Nutzer dürfen sich nicht selbst als Recruiter oder Admin registrieren.
            Role = AppRoles.Candidate,

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
        var normalizedEmail = NormalizeEmail(request.Email);

        var user = await _context.Users
            .FirstOrDefaultAsync(
                existingUser =>
                    existingUser.Email.ToLower() == normalizedEmail);

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

    public async Task<MessageResponse> ForgotPasswordAsync(
        ForgotPasswordRequest request)
    {
        var publicResponse = new MessageResponse
        {
            Message =
                "Falls ein Konto mit dieser E-Mail-Adresse existiert, " +
                "wurde ein Link zum Zurücksetzen des Passworts versendet."
        };

        var normalizedEmail = NormalizeEmail(request.Email);

        var user = await _context.Users
            .FirstOrDefaultAsync(
                existingUser =>
                    existingUser.Email.ToLower() == normalizedEmail);

        // Immer dieselbe Antwort zurückgeben.
        // Dadurch kann niemand registrierte E-Mail-Adressen herausfinden.
        if (user == null)
            return publicResponse;

        var now = DateTime.UtcNow;

        // Frühere, noch nicht verwendete Tokens ungültig machen.
        var oldTokens = await _context.PasswordResetTokens
            .Where(token =>
                token.UserId == user.Id &&
                token.UsedAt == null)
            .ToListAsync();

        foreach (var oldToken in oldTokens)
        {
            oldToken.UsedAt = now;
        }

        var rawToken = GenerateSecureToken();

        var passwordResetToken = new PasswordResetToken
        {
            UserId = user.Id,
            TokenHash = HashToken(rawToken),
            CreatedAt = now,
            ExpiresAt = now.AddMinutes(30)
        };

        _context.PasswordResetTokens.Add(passwordResetToken);

        await _context.SaveChangesAsync();

        var frontendUrl =
            _configuration["Frontend:BaseUrl"]
            ?? "http://localhost:5173";

        var resetLink =
            $"{frontendUrl.TrimEnd('/')}/reset-password" +
            $"?token={Uri.EscapeDataString(rawToken)}";

        try
        {
            await _emailService.SendEmailAsync(
                user.Email,
                "SkillJobAI – Passwort zurücksetzen",
                BuildPasswordResetEmail(user.FullName, resetLink));
        }
        catch (Exception exception)
        {
            _logger.LogError(
                exception,
                "Password reset email could not be sent for user {UserId}.",
                user.Id);

            // Das Token wird wieder ungültig gemacht, wenn die E-Mail
            // nicht versendet werden konnte.
            passwordResetToken.UsedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();
        }

        return publicResponse;
    }

    public async Task<PasswordResetResponse> ResetPasswordAsync(
        ResetPasswordRequest request)
    {
        var tokenValue = request.Token.Trim();

        if (string.IsNullOrWhiteSpace(tokenValue))
        {
            return new PasswordResetResponse
            {
                Success = false,
                Message = "Der Reset-Link ist ungültig."
            };
        }

        var tokenHash = HashToken(tokenValue);

        var resetToken = await _context.PasswordResetTokens
            .Include(token => token.User)
            .FirstOrDefaultAsync(token =>
                token.TokenHash == tokenHash &&
                token.UsedAt == null);

        if (resetToken == null)
        {
            return new PasswordResetResponse
            {
                Success = false,
                Message =
                    "Der Reset-Link ist ungültig oder wurde bereits verwendet."
            };
        }

        if (resetToken.ExpiresAt <= DateTime.UtcNow)
        {
            resetToken.UsedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();

            return new PasswordResetResponse
            {
                Success = false,
                Message = "Der Reset-Link ist abgelaufen."
            };
        }

        resetToken.User.PasswordHash =
            _passwordService.HashPassword(request.NewPassword);

        resetToken.UsedAt = DateTime.UtcNow;

        // Alle anderen offenen Tokens dieses Nutzers ebenfalls sperren.
        var otherActiveTokens = await _context.PasswordResetTokens
            .Where(token =>
                token.UserId == resetToken.UserId &&
                token.Id != resetToken.Id &&
                token.UsedAt == null)
            .ToListAsync();

        foreach (var otherToken in otherActiveTokens)
        {
            otherToken.UsedAt = DateTime.UtcNow;
        }

        await _context.SaveChangesAsync();

        return new PasswordResetResponse
        {
            Success = true,
            Message = "Das Passwort wurde erfolgreich geändert."
        };
    }

    private static string NormalizeEmail(string email)
    {
        return email.Trim().ToLowerInvariant();
    }

    private static string GenerateSecureToken()
    {
        var bytes = RandomNumberGenerator.GetBytes(32);

        return Convert.ToBase64String(bytes)
            .TrimEnd('=')
            .Replace('+', '-')
            .Replace('/', '_');
    }

    private static string HashToken(string token)
    {
        var bytes = Encoding.UTF8.GetBytes(token);
        var hash = SHA256.HashData(bytes);

        return Convert.ToHexString(hash);
    }

    private static string BuildPasswordResetEmail(
        string? fullName,
        string resetLink)
    {
        var greetingName = string.IsNullOrWhiteSpace(fullName)
            ? string.Empty
            : $" {fullName}";

        return $"""
                Hallo{greetingName},

                du hast angefordert, dein Passwort für SkillJobAI zurückzusetzen.

                Öffne diesen Link:

                {resetLink}

                Der Link ist 30 Minuten gültig und kann nur einmal verwendet werden.

                Falls du diese Anfrage nicht gestellt hast, kannst du diese E-Mail ignorieren.

                Viele Grüße
                Dein SkillJobAI-Team
                """;
    }
}