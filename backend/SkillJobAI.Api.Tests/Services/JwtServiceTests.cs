using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Microsoft.Extensions.Configuration;
using SkillJobAI.Api.Entities;
using SkillJobAI.Api.Services;

namespace SkillJobAI.Api.Tests.Services;

public class JwtServiceTests
{
    [Fact]
    public void GenerateToken_ShouldCreateValidToken_WithExpectedClaims()
    {
        // Arrange
        var configuration =
            CreateConfiguration();

        var jwtService =
            new JwtService(configuration);

        var user =
            new AppUser
            {
                Id = 42,
                FullName = "Test Candidate",
                Email = "candidate@test.com",
                Role = "Candidate"
            };

        // Act
        var tokenValue =
            jwtService.GenerateToken(user);

        // Assert
        Assert.False(
            string.IsNullOrWhiteSpace(
                tokenValue));

        var tokenHandler =
            new JwtSecurityTokenHandler();

        var token =
            tokenHandler.ReadJwtToken(
                tokenValue);

        Assert.Equal(
            "SkillJobAI.Tests",
            token.Issuer);

        Assert.Contains(
            "SkillJobAI.Tests",
            token.Audiences);

        Assert.Equal(
            user.Id.ToString(),
            token.Claims
                .Single(claim =>
                    claim.Type ==
                    ClaimTypes.NameIdentifier)
                .Value);

        Assert.Equal(
            user.FullName,
            token.Claims
                .Single(claim =>
                    claim.Type ==
                    ClaimTypes.Name)
                .Value);

        Assert.Equal(
            user.Email,
            token.Claims
                .Single(claim =>
                    claim.Type ==
                    ClaimTypes.Email)
                .Value);

        Assert.Equal(
            user.Role,
            token.Claims
                .Single(claim =>
                    claim.Type ==
                    ClaimTypes.Role)
                .Value);
    }

    [Fact]
    public void GenerateToken_ShouldCreateToken_WithFutureExpiration()
    {
        // Arrange
        var configuration =
            CreateConfiguration(
                expiresInMinutes: "30");

        var jwtService =
            new JwtService(configuration);

        var user =
            CreateUser();

        var beforeGeneration =
            DateTime.UtcNow;

        // Act
        var tokenValue =
            jwtService.GenerateToken(user);

        var token =
            new JwtSecurityTokenHandler()
                .ReadJwtToken(
                    tokenValue);

        var afterGeneration =
            DateTime.UtcNow;

        // Assert
        Assert.True(
            token.ValidTo >
            beforeGeneration.AddMinutes(29));

        Assert.True(
            token.ValidTo <=
            afterGeneration.AddMinutes(30)
                .AddSeconds(5));
    }

    [Fact]
    public void GetTokenExpirationUtc_ShouldUseConfiguredMinutes()
    {
        // Arrange
        var configuration =
            CreateConfiguration(
                expiresInMinutes: "20");

        var jwtService =
            new JwtService(configuration);

        var beforeCall =
            DateTime.UtcNow;

        // Act
        var result =
            jwtService.GetTokenExpirationUtc();

        var afterCall =
            DateTime.UtcNow;

        // Assert
        Assert.True(
            result >=
            beforeCall.AddMinutes(20));

        Assert.True(
            result <=
            afterCall.AddMinutes(20)
                .AddSeconds(1));
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("not-a-number")]
    public void GetTokenExpirationUtc_ShouldUseDefault_WhenValueIsInvalid(
        string? configuredValue)
    {
        // Arrange
        var configurationValues =
            new Dictionary<string, string?>
            {
                ["Jwt:Key"] =
                    "ThisIsATestJwtKeyThatIsLongEnoughForHmacSha256",

                ["Jwt:Issuer"] =
                    "SkillJobAI.Tests",

                ["Jwt:Audience"] =
                    "SkillJobAI.Tests",

                ["Jwt:ExpiresInMinutes"] =
                    configuredValue
            };

        var configuration =
            new ConfigurationBuilder()
                .AddInMemoryCollection(
                    configurationValues)
                .Build();

        var jwtService =
            new JwtService(configuration);

        var beforeCall =
            DateTime.UtcNow;

        // Act
        var result =
            jwtService.GetTokenExpirationUtc();

        var afterCall =
            DateTime.UtcNow;

        // Assert
        Assert.True(
            result >=
            beforeCall.AddMinutes(15));

        Assert.True(
            result <=
            afterCall.AddMinutes(15)
                .AddSeconds(1));
    }

    [Fact]
    public void GenerateToken_ShouldThrow_WhenJwtKeyIsMissing()
    {
        // Arrange
        var configuration =
            CreateConfiguration(
                includeKey: false);

        var jwtService =
            new JwtService(configuration);

        var user =
            CreateUser();

        // Act
        var exception =
            Assert.Throws<InvalidOperationException>(
                () =>
                    jwtService.GenerateToken(
                        user));

        // Assert
        Assert.Equal(
            "Die Konfiguration 'Jwt:Key' fehlt.",
            exception.Message);
    }

    [Fact]
    public void GenerateToken_ShouldThrow_WhenIssuerIsMissing()
    {
        // Arrange
        var configuration =
            CreateConfiguration(
                includeIssuer: false);

        var jwtService =
            new JwtService(configuration);

        var user =
            CreateUser();

        // Act
        var exception =
            Assert.Throws<InvalidOperationException>(
                () =>
                    jwtService.GenerateToken(
                        user));

        // Assert
        Assert.Equal(
            "Die Konfiguration 'Jwt:Issuer' fehlt.",
            exception.Message);
    }

    [Fact]
    public void GenerateToken_ShouldThrow_WhenAudienceIsMissing()
    {
        // Arrange
        var configuration =
            CreateConfiguration(
                includeAudience: false);

        var jwtService =
            new JwtService(configuration);

        var user =
            CreateUser();

        // Act
        var exception =
            Assert.Throws<InvalidOperationException>(
                () =>
                    jwtService.GenerateToken(
                        user));

        // Assert
        Assert.Equal(
            "Die Konfiguration 'Jwt:Audience' fehlt.",
            exception.Message);
    }

    [Fact]
    public void GenerateToken_ShouldCreateDifferentTokens_ForDifferentUsers()
    {
        // Arrange
        var configuration =
            CreateConfiguration();

        var jwtService =
            new JwtService(configuration);

        var firstUser =
            new AppUser
            {
                Id = 1,
                FullName = "First User",
                Email = "first@test.com",
                Role = "Candidate"
            };

        var secondUser =
            new AppUser
            {
                Id = 2,
                FullName = "Second User",
                Email = "second@test.com",
                Role = "Admin"
            };

        // Act
        var firstToken =
            jwtService.GenerateToken(
                firstUser);

        var secondToken =
            jwtService.GenerateToken(
                secondUser);

        // Assert
        Assert.NotEqual(
            firstToken,
            secondToken);
    }

    private static AppUser CreateUser()
    {
        return new AppUser
        {
            Id = 1,
            FullName = "Test User",
            Email = "test@test.com",
            Role = "Candidate"
        };
    }

    private static IConfiguration CreateConfiguration(
        string expiresInMinutes = "15",
        bool includeKey = true,
        bool includeIssuer = true,
        bool includeAudience = true)
    {
        var configurationValues =
            new Dictionary<string, string?>
            {
                ["Jwt:ExpiresInMinutes"] =
                    expiresInMinutes
            };

        if (includeKey)
        {
            configurationValues["Jwt:Key"] =
                "ThisIsATestJwtKeyThatIsLongEnoughForHmacSha256";
        }

        if (includeIssuer)
        {
            configurationValues["Jwt:Issuer"] =
                "SkillJobAI.Tests";
        }

        if (includeAudience)
        {
            configurationValues["Jwt:Audience"] =
                "SkillJobAI.Tests";
        }

        return new ConfigurationBuilder()
            .AddInMemoryCollection(
                configurationValues)
            .Build();
    }
}