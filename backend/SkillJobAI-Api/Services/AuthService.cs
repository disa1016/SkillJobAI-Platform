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
            .AnyAsync(user =>
                user.Email.ToLower() == normalizedEmail);

        if (emailExists)
        {
            _logger.LogWarning(
                "Registrierung abgelehnt. E-Mail {Email} ist bereits registriert.",
                normalizedEmail);

            return null;
        }

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

        var rawRefreshToken = GenerateSecureToken();

        var refreshToken = CreateRefreshToken(
            user.Id,
            rawRefreshToken);

        _context.RefreshTokens.Add(refreshToken);
        await _context.SaveChangesAsync();

        var accessToken = _jwtService.GenerateToken(user);

        _logger.LogInformation(
            "Neuer Benutzer registriert. UserId: {UserId}, Rolle: {Role}",
            user.Id,
            user.Role);

        return CreateAuthResponse(
            "User registered successfully",
            user,
            accessToken,
            rawRefreshToken);
    }

    public async Task<AuthResponse?> LoginAsync(LoginRequest request)
    {
        var normalizedEmail = NormalizeEmail(request.Email);

        var user = await _context.Users
            .FirstOrDefaultAsync(existingUser =>
                existingUser.Email.ToLower() == normalizedEmail);

        if (user == null)
        {
            _logger.LogWarning(
                "Login fehlgeschlagen. Benutzer mit E-Mail {Email} wurde nicht gefunden.",
                normalizedEmail);

            return null;
        }

        var passwordIsValid = _passwordService.VerifyPassword(
            request.Password,
            user.PasswordHash);

        if (!passwordIsValid)
        {
            _logger.LogWarning(
                "Login fehlgeschlagen. Falsches Passwort für UserId {UserId}.",
                user.Id);

            return null;
        }

        var rawRefreshToken = GenerateSecureToken();

        var refreshToken = CreateRefreshToken(
            user.Id,
            rawRefreshToken);

        _context.RefreshTokens.Add(refreshToken);
        await _context.SaveChangesAsync();

        var accessToken = _jwtService.GenerateToken(user);

        _logger.LogInformation(
            "Benutzer erfolgreich angemeldet. UserId: {UserId}, Rolle: {Role}",
            user.Id,
            user.Role);

        return CreateAuthResponse(
            "Login successful",
            user,
            accessToken,
            rawRefreshToken);
    }

    public async Task<AuthResponse?> RefreshTokenAsync(
        RefreshTokenRequest request)
    {
        var rawToken = request.RefreshToken?.Trim();

        if (string.IsNullOrWhiteSpace(rawToken))
        {
            _logger.LogWarning(
                "Token-Aktualisierung mit leerem Refresh Token angefordert.");

            return null;
        }

        var tokenHash = HashToken(rawToken);

        var storedToken = await _context.RefreshTokens
            .Include(token => token.User)
            .FirstOrDefaultAsync(token =>
                token.TokenHash == tokenHash);

        if (storedToken == null)
        {
            _logger.LogWarning(
                "Unbekannter Refresh Token wurde verwendet.");

            return null;
        }

        var now = DateTime.UtcNow;

        /*
         * Reuse Detection:
         * Wird ein bereits widerrufener Refresh Token erneut verwendet,
         * werden vorsichtshalber alle noch aktiven Sessions des Benutzers
         * widerrufen.
         */
        if (storedToken.RevokedAt != null)
        {
            await RevokeAllActiveRefreshTokensAsync(
                storedToken.UserId,
                now);

            await _context.SaveChangesAsync();

            _logger.LogWarning(
                "Wiederverwendung eines widerrufenen Refresh Tokens erkannt. " +
                "Alle Sessions wurden gesperrt. UserId: {UserId}",
                storedToken.UserId);

            return null;
        }

        if (storedToken.ExpiresAt <= now)
        {
            storedToken.RevokedAt = now;

            await _context.SaveChangesAsync();

            _logger.LogWarning(
                "Abgelaufener Refresh Token wurde verwendet. UserId: {UserId}",
                storedToken.UserId);

            return null;
        }

        await using var transaction =
            await _context.Database.BeginTransactionAsync();

        try
        {
            var newRawRefreshToken = GenerateSecureToken();

            var newRefreshToken = CreateRefreshToken(
                storedToken.UserId,
                newRawRefreshToken);

            // Der alte Token wird durch den neuen Token ersetzt.
            storedToken.RevokedAt = now;
            storedToken.ReplacedByTokenHash =
                newRefreshToken.TokenHash;

            _context.RefreshTokens.Add(newRefreshToken);

            await _context.SaveChangesAsync();
            await transaction.CommitAsync();

            var accessToken =
                _jwtService.GenerateToken(storedToken.User);

            _logger.LogInformation(
                "Refresh Token erfolgreich rotiert. UserId: {UserId}",
                storedToken.UserId);

            return CreateAuthResponse(
                "Token refreshed successfully",
                storedToken.User,
                accessToken,
                newRawRefreshToken);
        }
        catch (Exception exception)
        {
            await transaction.RollbackAsync();

            _logger.LogError(
                exception,
                "Fehler bei der Refresh-Token-Rotation. UserId: {UserId}",
                storedToken.UserId);

            throw;
        }
    }

    public async Task<bool> LogoutAsync(LogoutRequest request)
    {
        var rawToken = request.RefreshToken?.Trim();

        if (string.IsNullOrWhiteSpace(rawToken))
        {
            _logger.LogWarning(
                "Logout mit leerem Refresh Token angefordert.");

            return false;
        }

        var tokenHash = HashToken(rawToken);

        var storedToken = await _context.RefreshTokens
            .FirstOrDefaultAsync(token =>
                token.TokenHash == tokenHash);

        if (storedToken == null)
        {
            _logger.LogWarning(
                "Logout mit unbekanntem Refresh Token angefordert.");

            return false;
        }

        if (storedToken.RevokedAt != null)
        {
            _logger.LogWarning(
                "Logout mit bereits widerrufenem Refresh Token angefordert. " +
                "UserId: {UserId}",
                storedToken.UserId);

            return false;
        }

        storedToken.RevokedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();

        _logger.LogInformation(
            "Benutzer erfolgreich abgemeldet. UserId: {UserId}",
            storedToken.UserId);

        return true;
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
            .FirstOrDefaultAsync(existingUser =>
                existingUser.Email.ToLower() == normalizedEmail);

        // Immer dieselbe Antwort zurückgeben.
        // Dadurch kann niemand registrierte E-Mail-Adressen herausfinden.
        if (user == null)
        {
            _logger.LogWarning(
                "Passwort-Reset für unbekannte E-Mail angefordert: {Email}",
                normalizedEmail);

            return publicResponse;
        }

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

            _logger.LogInformation(
                "Passwort-Reset-E-Mail erfolgreich versendet. UserId: {UserId}",
                user.Id);
        }
        catch (Exception exception)
        {
            _logger.LogError(
                exception,
                "Passwort-Reset-E-Mail konnte nicht versendet werden. UserId: {UserId}",
                user.Id);

            // Token ungültig machen, wenn die E-Mail nicht versendet wurde.
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
            _logger.LogWarning(
                "Passwort-Reset mit leerem Token angefordert.");

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
            _logger.LogWarning(
                "Passwort-Reset mit ungültigem oder bereits verwendetem Token angefordert.");

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

            _logger.LogWarning(
                "Abgelaufener Passwort-Reset-Token verwendet. UserId: {UserId}",
                resetToken.UserId);

            return new PasswordResetResponse
            {
                Success = false,
                Message = "Der Reset-Link ist abgelaufen."
            };
        }

        await using var transaction =
            await _context.Database.BeginTransactionAsync();

        try
        {
            var now = DateTime.UtcNow;

            resetToken.User.PasswordHash =
                _passwordService.HashPassword(request.NewPassword);

            resetToken.UsedAt = now;

            // Alle anderen offenen Passwort-Reset-Tokens sperren.
            var otherActivePasswordResetTokens =
                await _context.PasswordResetTokens
                    .Where(token =>
                        token.UserId == resetToken.UserId &&
                        token.Id != resetToken.Id &&
                        token.UsedAt == null)
                    .ToListAsync();

            foreach (var otherToken in otherActivePasswordResetTokens)
            {
                otherToken.UsedAt = now;
            }

            // Nach einer Passwortänderung alle aktiven Sessions widerrufen.
            await RevokeAllActiveRefreshTokensAsync(
                resetToken.UserId,
                now);

            await _context.SaveChangesAsync();
            await transaction.CommitAsync();

            _logger.LogInformation(
                "Passwort erfolgreich zurückgesetzt. " +
                "Alle aktiven Sessions wurden widerrufen. UserId: {UserId}",
                resetToken.UserId);

            return new PasswordResetResponse
            {
                Success = true,
                Message = "Das Passwort wurde erfolgreich geändert."
            };
        }
        catch (Exception exception)
        {
            await transaction.RollbackAsync();

            _logger.LogError(
                exception,
                "Fehler beim Zurücksetzen des Passworts. UserId: {UserId}",
                resetToken.UserId);

            throw;
        }
    }

    private AuthResponse CreateAuthResponse(
        string message,
        AppUser user,
        string accessToken,
        string refreshToken)
    {
        return new AuthResponse
        {
            Message = message,
            Token = accessToken,
            RefreshToken = refreshToken,
            TokenExpiresAt = _jwtService.GetTokenExpirationUtc(),
            User = new AuthUserResponse
            {
                Id = user.Id,
                FullName = user.FullName,
                Email = user.Email,
                Role = user.Role
            }
        };
    }

    private RefreshToken CreateRefreshToken(
        int userId,
        string rawToken)
    {
        var now = DateTime.UtcNow;

        return new RefreshToken
        {
            UserId = userId,
            TokenHash = HashToken(rawToken),
            CreatedAt = now,
            ExpiresAt = now.AddDays(GetRefreshTokenExpirationDays())
        };
    }

    private async Task RevokeAllActiveRefreshTokensAsync(
        int userId,
        DateTime revokedAt)
    {
        var activeTokens = await _context.RefreshTokens
            .Where(token =>
                token.UserId == userId &&
                token.RevokedAt == null)
            .ToListAsync();

        foreach (var activeToken in activeTokens)
        {
            activeToken.RevokedAt = revokedAt;
        }
    }

    private int GetRefreshTokenExpirationDays()
    {
        var configuredValue =
            _configuration["Jwt:RefreshTokenExpiresInDays"];

        if (!int.TryParse(
                configuredValue,
                out var expirationDays) ||
            expirationDays <= 0)
        {
            expirationDays = 30;
        }

        return expirationDays;
    }

    private static string NormalizeEmail(string email)
    {
        return email
            .Trim()
            .ToLowerInvariant();
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