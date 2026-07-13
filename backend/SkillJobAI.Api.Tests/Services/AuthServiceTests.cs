using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Moq;
using SkillJobAI.Api.Entities;
using SkillJobAI.Api.Models;
using SkillJobAI.Api.Services;
using SkillJobAI.Api.Tests.Helpers;

namespace SkillJobAI.Api.Tests.Services;

public class AuthServiceTests
{
    [Fact]
    public async Task LoginAsync_ShouldReturnNull_WhenUserDoesNotExist()
    {
        // Arrange
        await using var context =
            TestDbContextFactory.Create();

        var configuration =
            CreateTestConfiguration();

        var jwtService =
            new JwtService(configuration);

        var passwordService =
            new PasswordService();

        var emailServiceMock =
            new Mock<IEmailService>(
                MockBehavior.Strict);

        var loggerMock =
            new Mock<ILogger<AuthService>>();

        var authService =
            new AuthService(
                context,
                jwtService,
                passwordService,
                emailServiceMock.Object,
                configuration,
                loggerMock.Object);

        var request =
            new LoginRequest
            {
                Email = "unknown@test.com",
                Password = "WrongPassword123!"
            };

        // Act
        var result =
            await authService.LoginAsync(
                request);

        // Assert
        Assert.Null(result);

        Assert.Empty(
            await context.RefreshTokens
                .ToListAsync());

        emailServiceMock.VerifyNoOtherCalls();
    }

    [Fact]
    public async Task LoginAsync_ShouldReturnNull_WhenPasswordIsIncorrect()
    {
        // Arrange
        await using var context =
            TestDbContextFactory.Create();

        var configuration =
            CreateTestConfiguration();

        var passwordService =
            new PasswordService();

        var user =
            new AppUser
            {
                FullName = "Test Candidate",
                Email = "candidate@test.com",
                PasswordHash =
                    passwordService.HashPassword(
                        "CorrectPassword123!"),
                Role = "Candidate",
                CreatedAt = DateTime.UtcNow
            };

        context.Users.Add(user);

        await context.SaveChangesAsync();

        var emailServiceMock =
            new Mock<IEmailService>(
                MockBehavior.Strict);

        var authService =
            new AuthService(
                context,
                new JwtService(configuration),
                passwordService,
                emailServiceMock.Object,
                configuration,
                new Mock<ILogger<AuthService>>()
                    .Object);

        var request =
            new LoginRequest
            {
                Email = "candidate@test.com",
                Password = "WrongPassword123!"
            };

        // Act
        var result =
            await authService.LoginAsync(
                request);

        // Assert
        Assert.Null(result);

        Assert.Empty(
            await context.RefreshTokens
                .ToListAsync());

        emailServiceMock.VerifyNoOtherCalls();
    }

    [Fact]
    public async Task LoginAsync_ShouldReturnAuthResponse_WhenCredentialsAreCorrect()
    {
        // Arrange
        await using var context =
            TestDbContextFactory.Create();

        var configuration =
            CreateTestConfiguration();

        var passwordService =
            new PasswordService();

        var user =
            new AppUser
            {
                FullName = "Test Candidate",
                Email = "candidate@test.com",
                PasswordHash =
                    passwordService.HashPassword(
                        "CorrectPassword123!"),
                Role = "Candidate",
                CreatedAt = DateTime.UtcNow
            };

        context.Users.Add(user);

        await context.SaveChangesAsync();

        var authService =
            new AuthService(
                context,
                new JwtService(configuration),
                passwordService,
                new Mock<IEmailService>().Object,
                configuration,
                new Mock<ILogger<AuthService>>()
                    .Object);

        var request =
            new LoginRequest
            {
                Email = "candidate@test.com",
                Password = "CorrectPassword123!"
            };

        // Act
        var result =
            await authService.LoginAsync(
                request);

        // Assert
        Assert.NotNull(result);

        Assert.Equal(
            "Login successful",
            result.Message);

        Assert.False(
            string.IsNullOrWhiteSpace(
                result.Token));

        Assert.False(
            string.IsNullOrWhiteSpace(
                result.RefreshToken));

        Assert.True(
            result.TokenExpiresAt >
            DateTime.UtcNow);

        Assert.Equal(
            user.Id,
            result.User.Id);

        Assert.Equal(
            user.FullName,
            result.User.FullName);

        Assert.Equal(
            user.Email,
            result.User.Email);

        Assert.Equal(
            user.Role,
            result.User.Role);

        var storedRefreshToken =
            await context.RefreshTokens
                .SingleAsync();

        Assert.Equal(
            user.Id,
            storedRefreshToken.UserId);

        Assert.False(
            string.IsNullOrWhiteSpace(
                storedRefreshToken.TokenHash));

        Assert.NotEqual(
            result.RefreshToken,
            storedRefreshToken.TokenHash);

        Assert.Null(
            storedRefreshToken.RevokedAt);

        Assert.True(
            storedRefreshToken.ExpiresAt >
            storedRefreshToken.CreatedAt);
    }

    [Fact]
    public async Task RefreshTokenAsync_ShouldRotateRefreshToken_WhenTokenIsValid()
    {
        // Arrange
        await using var context =
            TestDbContextFactory.Create(
                ignoreTransactionWarnings: true);

        var configuration =
            CreateTestConfiguration();

        var passwordService =
            new PasswordService();

        var user =
            new AppUser
            {
                FullName = "Test Candidate",
                Email = "candidate@test.com",
                PasswordHash =
                    passwordService.HashPassword(
                        "CorrectPassword123!"),
                Role = "Candidate",
                CreatedAt = DateTime.UtcNow
            };

        context.Users.Add(user);

        await context.SaveChangesAsync();

        var authService =
            new AuthService(
                context,
                new JwtService(configuration),
                passwordService,
                new Mock<IEmailService>().Object,
                configuration,
                new Mock<ILogger<AuthService>>()
                    .Object);

        var loginResult =
            await authService.LoginAsync(
                new LoginRequest
                {
                    Email =
                        "candidate@test.com",
                    Password =
                        "CorrectPassword123!"
                });

        Assert.NotNull(loginResult);

        var oldRefreshTokenValue =
            loginResult.RefreshToken;

        // Act
        var refreshResult =
            await authService.RefreshTokenAsync(
                new RefreshTokenRequest
                {
                    RefreshToken =
                        oldRefreshTokenValue
                });

        // Assert
        Assert.NotNull(refreshResult);

        Assert.False(
            string.IsNullOrWhiteSpace(
                refreshResult.Token));

        Assert.False(
            string.IsNullOrWhiteSpace(
                refreshResult.RefreshToken));

        Assert.NotEqual(
            oldRefreshTokenValue,
            refreshResult.RefreshToken);

        var storedTokens =
            await context.RefreshTokens
                .OrderBy(token => token.Id)
                .ToListAsync();

        Assert.Equal(
            2,
            storedTokens.Count);

        var revokedOldToken =
            storedTokens[0];

        var newStoredToken =
            storedTokens[1];

        Assert.NotNull(
            revokedOldToken.RevokedAt);

        Assert.False(
            string.IsNullOrWhiteSpace(
                revokedOldToken
                    .ReplacedByTokenHash));

        Assert.Null(
            newStoredToken.RevokedAt);

        Assert.Equal(
            revokedOldToken
                .ReplacedByTokenHash,
            newStoredToken.TokenHash);

        Assert.NotEqual(
            refreshResult.RefreshToken,
            newStoredToken.TokenHash);

        Assert.True(
            newStoredToken.ExpiresAt >
            newStoredToken.CreatedAt);
    }

    [Fact]
    public async Task RefreshTokenAsync_ShouldRevokeAllSessions_WhenRevokedTokenIsReused()
    {
        // Arrange
        await using var context =
            TestDbContextFactory.Create(
                ignoreTransactionWarnings: true);

        var configuration =
            CreateTestConfiguration();

        var passwordService =
            new PasswordService();

        var user =
            new AppUser
            {
                FullName = "Test Candidate",
                Email = "candidate@test.com",
                PasswordHash =
                    passwordService.HashPassword(
                        "CorrectPassword123!"),
                Role = "Candidate",
                CreatedAt = DateTime.UtcNow
            };

        context.Users.Add(user);

        await context.SaveChangesAsync();

        var authService =
            new AuthService(
                context,
                new JwtService(configuration),
                passwordService,
                new Mock<IEmailService>().Object,
                configuration,
                new Mock<ILogger<AuthService>>()
                    .Object);

        var loginResult =
            await authService.LoginAsync(
                new LoginRequest
                {
                    Email =
                        "candidate@test.com",
                    Password =
                        "CorrectPassword123!"
                });

        Assert.NotNull(loginResult);

        var oldRefreshToken =
            loginResult.RefreshToken;

        var firstRefreshResult =
            await authService.RefreshTokenAsync(
                new RefreshTokenRequest
                {
                    RefreshToken =
                        oldRefreshToken
                });

        Assert.NotNull(firstRefreshResult);

        var newRefreshToken =
            firstRefreshResult.RefreshToken;

        // Act
        var reuseResult =
            await authService.RefreshTokenAsync(
                new RefreshTokenRequest
                {
                    RefreshToken =
                        oldRefreshToken
                });

        // Assert
        Assert.Null(reuseResult);

        var storedTokens =
            await context.RefreshTokens
                .OrderBy(token => token.Id)
                .ToListAsync();

        Assert.Equal(
            2,
            storedTokens.Count);

        Assert.All(
            storedTokens,
            token =>
                Assert.NotNull(
                    token.RevokedAt));

        var newTokenResult =
            await authService.RefreshTokenAsync(
                new RefreshTokenRequest
                {
                    RefreshToken =
                        newRefreshToken
                });

        Assert.Null(newTokenResult);
    }

    private static IConfiguration
        CreateTestConfiguration()
    {
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
                    "15",

                ["Jwt:RefreshTokenExpiresInDays"] =
                    "30"
            };

        return new ConfigurationBuilder()
            .AddInMemoryCollection(
                configurationValues)
            .Build();
    }
}