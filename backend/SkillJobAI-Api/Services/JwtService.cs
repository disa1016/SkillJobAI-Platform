using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.IdentityModel.Tokens;
using SkillJobAI.Api.Entities;

namespace SkillJobAI.Api.Services;

public class JwtService
{
    private readonly IConfiguration _configuration;

    public JwtService(IConfiguration configuration)
    {
        _configuration = configuration;
    }

    public string GenerateToken(AppUser user)
    {
        var jwtKey = _configuration["Jwt:Key"]
            ?? throw new InvalidOperationException(
                "Die Konfiguration 'Jwt:Key' fehlt.");

        var issuer = _configuration["Jwt:Issuer"]
            ?? throw new InvalidOperationException(
                "Die Konfiguration 'Jwt:Issuer' fehlt.");

        var audience = _configuration["Jwt:Audience"]
            ?? throw new InvalidOperationException(
                "Die Konfiguration 'Jwt:Audience' fehlt.");

        var expiresAt = GetTokenExpirationUtc();

        var claims = new[]
        {
            new Claim(
                ClaimTypes.NameIdentifier,
                user.Id.ToString()),

            new Claim(
                ClaimTypes.Name,
                user.FullName),

            new Claim(
                ClaimTypes.Email,
                user.Email),

            new Claim(
                ClaimTypes.Role,
                user.Role)
        };

        var key = new SymmetricSecurityKey(
            Encoding.UTF8.GetBytes(jwtKey));

        var credentials = new SigningCredentials(
            key,
            SecurityAlgorithms.HmacSha256);

        var token = new JwtSecurityToken(
            issuer: issuer,
            audience: audience,
            claims: claims,
            expires: expiresAt,
            signingCredentials: credentials);

        return new JwtSecurityTokenHandler()
            .WriteToken(token);
    }

    public DateTime GetTokenExpirationUtc()
    {
        var expiresValue =
            _configuration["Jwt:ExpiresInMinutes"];

        if (!int.TryParse(expiresValue, out var expiresInMinutes))
        {
            expiresInMinutes = 15;
        }

        return DateTime.UtcNow.AddMinutes(expiresInMinutes);
    }
}