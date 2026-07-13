using System.Net;
using System.Net.Http.Json;
using System.Security.Cryptography;
using System.Text;
using Microsoft.EntityFrameworkCore;
using SkillJobAI.Api.Entities;
using SkillJobAI.Api.Models;
using SkillJobAI.Api.Models.Responses;

namespace SkillJobAI.Api.Tests.Integration;

public class PasswordResetEndpointsTests
: IClassFixture<CustomWebApplicationFactory>
{
private const string PublicForgotPasswordMessage =
"Falls ein Konto mit dieser E-Mail-Adresse existiert, " +
"wurde ein Link zum Zurücksetzen des Passworts versendet.";


private readonly CustomWebApplicationFactory
    _factory;

private readonly HttpClient
    _client;

public PasswordResetEndpointsTests(
    CustomWebApplicationFactory factory)
{
    _factory = factory;
    _client = factory.CreateClient();
}

[Fact]
public async Task ForgotPassword_ShouldReturnOk_WhenUserDoesNotExist()
{
    // Arrange
    await _factory.ResetDatabaseAsync();

    var request =
        new ForgotPasswordRequest
        {
            Email = "unknown@test.com"
        };

    // Act
    var response =
        await _client.PostAsJsonAsync(
            "/api/auth/forgot-password",
            request);

    // Assert
    Assert.Equal(
        HttpStatusCode.OK,
        response.StatusCode);

    var result =
        await response.Content
            .ReadFromJsonAsync<MessageResponse>();

    Assert.NotNull(result);

    Assert.Equal(
        PublicForgotPasswordMessage,
        result.Message);

    Assert.Empty(
        _factory
            .GetFakeEmailService()
            .Messages);

    await _factory.ExecuteDbContextAsync(
        async context =>
        {
            Assert.Empty(
                await context.PasswordResetTokens
                    .ToListAsync());
        });
}

[Fact]
public async Task ForgotPassword_ShouldCreateTokenAndSendEmail_WhenUserExists()
{
    // Arrange
    await _factory.ResetDatabaseAsync();

    var registration =
        await RegisterUserAsync();

    // Act
    var response =
        await _client.PostAsJsonAsync(
            "/api/auth/forgot-password",
            new ForgotPasswordRequest
            {
                Email =
                    registration.User.Email
            });

    // Assert
    Assert.Equal(
        HttpStatusCode.OK,
        response.StatusCode);

    var result =
        await response.Content
            .ReadFromJsonAsync<MessageResponse>();

    Assert.NotNull(result);

    Assert.Equal(
        PublicForgotPasswordMessage,
        result.Message);

    var emailService =
        _factory.GetFakeEmailService();

    var email =
        Assert.Single(
            emailService.Messages);

    Assert.Equal(
        registration.User.Email,
        email.ToEmail);

    Assert.Equal(
        "SkillJobAI – Passwort zurücksetzen",
        email.Subject);

    Assert.Contains(
        "/reset-password?token=",
        email.Body);

    var rawToken =
        ExtractResetToken(email.Body);

    Assert.False(
        string.IsNullOrWhiteSpace(
            rawToken));

    await _factory.ExecuteDbContextAsync(
        async context =>
        {
            var storedToken =
                await context.PasswordResetTokens
                    .SingleAsync();

            Assert.Equal(
                registration.User.Id,
                storedToken.UserId);

            Assert.Equal(
                HashToken(rawToken),
                storedToken.TokenHash);

            Assert.Null(
                storedToken.UsedAt);

            Assert.True(
                storedToken.ExpiresAt >
                storedToken.CreatedAt);
        });
}

[Fact]
public async Task ForgotPassword_ShouldInvalidatePreviousOpenToken()
{
    // Arrange
    await _factory.ResetDatabaseAsync();

    var registration =
        await RegisterUserAsync();

    var request =
        new ForgotPasswordRequest
        {
            Email =
                registration.User.Email
        };

    var firstResponse =
        await _client.PostAsJsonAsync(
            "/api/auth/forgot-password",
            request);

    Assert.Equal(
        HttpStatusCode.OK,
        firstResponse.StatusCode);

    // Act
    var secondResponse =
        await _client.PostAsJsonAsync(
            "/api/auth/forgot-password",
            request);

    // Assert
    Assert.Equal(
        HttpStatusCode.OK,
        secondResponse.StatusCode);

    await _factory.ExecuteDbContextAsync(
        async context =>
        {
            var tokens =
                await context.PasswordResetTokens
                    .OrderBy(token => token.Id)
                    .ToListAsync();

            Assert.Equal(
                2,
                tokens.Count);

            Assert.NotNull(
                tokens[0].UsedAt);

            Assert.Null(
                tokens[1].UsedAt);
        });
}

[Fact]
public async Task ResetPassword_ShouldReturnBadRequest_WhenTokenIsInvalid()
{
    // Arrange
    await _factory.ResetDatabaseAsync();

    var request =
        new ResetPasswordRequest
        {
            Token = "invalid-token",
            NewPassword =
                "NewSecurePassword123!",
            ConfirmPassword =
                "NewSecurePassword123!"
        };

    // Act
    var response =
        await _client.PostAsJsonAsync(
            "/api/auth/reset-password",
            request);

    // Assert
    Assert.Equal(
        HttpStatusCode.BadRequest,
        response.StatusCode);

    var result =
        await response.Content
            .ReadFromJsonAsync<
                PasswordResetResponse>();

    Assert.NotNull(result);
    Assert.False(result.Success);

    Assert.Equal(
        "Der Reset-Link ist ungültig oder wurde bereits verwendet.",
        result.Message);
}

[Fact]
public async Task ResetPassword_ShouldReturnBadRequest_WhenTokenIsExpired()
{
    // Arrange
    await _factory.ResetDatabaseAsync();

    var registration =
        await RegisterUserAsync();

    const string rawToken =
        "expired-password-reset-token";

    await _factory.SeedAsync(
        async context =>
        {
            context.PasswordResetTokens.Add(
                new PasswordResetToken
                {
                    UserId =
                        registration.User.Id,

                    TokenHash =
                        HashToken(rawToken),

                    CreatedAt =
                        DateTime.UtcNow
                            .AddHours(-1),

                    ExpiresAt =
                        DateTime.UtcNow
                            .AddMinutes(-1)
                });

            await context.SaveChangesAsync();
        });

    var request =
        new ResetPasswordRequest
        {
            Token = rawToken,
            NewPassword =
                "NewSecurePassword123!",
            ConfirmPassword =
                "NewSecurePassword123!"
        };

    // Act
    var response =
        await _client.PostAsJsonAsync(
            "/api/auth/reset-password",
            request);

    // Assert
    Assert.Equal(
        HttpStatusCode.BadRequest,
        response.StatusCode);

    var result =
        await response.Content
            .ReadFromJsonAsync<
                PasswordResetResponse>();

    Assert.NotNull(result);
    Assert.False(result.Success);

    Assert.Equal(
        "Der Reset-Link ist abgelaufen.",
        result.Message);

    await _factory.ExecuteDbContextAsync(
        async context =>
        {
            var storedToken =
                await context.PasswordResetTokens
                    .SingleAsync();

            Assert.NotNull(
                storedToken.UsedAt);
        });
}

[Fact]
public async Task ResetPassword_ShouldReturnBadRequest_WhenPasswordsDoNotMatch()
{
    // Arrange
    await _factory.ResetDatabaseAsync();

    var request =
        new ResetPasswordRequest
        {
            Token = "some-token",
            NewPassword =
                "NewSecurePassword123!",
            ConfirmPassword =
                "DifferentPassword123!"
        };

    // Act
    var response =
        await _client.PostAsJsonAsync(
            "/api/auth/reset-password",
            request);

    // Assert
    Assert.Equal(
        HttpStatusCode.BadRequest,
        response.StatusCode);
}

[Fact]
public async Task ResetPassword_ShouldChangePasswordAndRevokeSessions()
{
    // Arrange
    await _factory.ResetDatabaseAsync();

    const string oldPassword =
        "OldSecurePassword123!";

    const string newPassword =
        "NewSecurePassword123!";

    var registration =
        await RegisterUserAsync(
            password: oldPassword);

    var loginResponse =
        await _client.PostAsJsonAsync(
            "/api/auth/login",
            new LoginRequest
            {
                Email =
                    registration.User.Email,

                Password =
                    oldPassword
            });

    Assert.Equal(
        HttpStatusCode.OK,
        loginResponse.StatusCode);

    var forgotResponse =
        await _client.PostAsJsonAsync(
            "/api/auth/forgot-password",
            new ForgotPasswordRequest
            {
                Email =
                    registration.User.Email
            });

    Assert.Equal(
        HttpStatusCode.OK,
        forgotResponse.StatusCode);

    var email =
        _factory
            .GetFakeEmailService()
            .LastMessage;

    Assert.NotNull(email);

    var rawToken =
        ExtractResetToken(
            email.Body);

    // Act
    var resetResponse =
        await _client.PostAsJsonAsync(
            "/api/auth/reset-password",
            new ResetPasswordRequest
            {
                Token =
                    rawToken,

                NewPassword =
                    newPassword,

                ConfirmPassword =
                    newPassword
            });

    // Assert
    Assert.Equal(
        HttpStatusCode.OK,
        resetResponse.StatusCode);

    var resetResult =
        await resetResponse.Content
            .ReadFromJsonAsync<
                PasswordResetResponse>();

    Assert.NotNull(resetResult);
    Assert.True(resetResult.Success);

    Assert.Equal(
        "Das Passwort wurde erfolgreich geändert.",
        resetResult.Message);

    await _factory.ExecuteDbContextAsync(
        async context =>
        {
            var resetToken =
                await context.PasswordResetTokens
                    .SingleAsync();

            Assert.NotNull(
                resetToken.UsedAt);

            var refreshTokens =
                await context.RefreshTokens
                    .ToListAsync();

            Assert.Equal(
                2,
                refreshTokens.Count);

            Assert.All(
                refreshTokens,
                token =>
                    Assert.NotNull(
                        token.RevokedAt));
        });

    var oldPasswordLogin =
        await _client.PostAsJsonAsync(
            "/api/auth/login",
            new LoginRequest
            {
                Email =
                    registration.User.Email,

                Password =
                    oldPassword
            });

    Assert.Equal(
        HttpStatusCode.Unauthorized,
        oldPasswordLogin.StatusCode);

    var newPasswordLogin =
        await _client.PostAsJsonAsync(
            "/api/auth/login",
            new LoginRequest
            {
                Email =
                    registration.User.Email,

                Password =
                    newPassword
            });

    Assert.Equal(
        HttpStatusCode.OK,
        newPasswordLogin.StatusCode);
}

[Fact]
public async Task ResetPassword_ShouldRejectTokenAfterSuccessfulUse()
{
    // Arrange
    await _factory.ResetDatabaseAsync();

    var registration =
        await RegisterUserAsync();

    await _client.PostAsJsonAsync(
        "/api/auth/forgot-password",
        new ForgotPasswordRequest
        {
            Email =
                registration.User.Email
        });

    var email =
        _factory
            .GetFakeEmailService()
            .LastMessage;

    Assert.NotNull(email);

    var rawToken =
        ExtractResetToken(
            email.Body);

    var request =
        new ResetPasswordRequest
        {
            Token =
                rawToken,

            NewPassword =
                "NewSecurePassword123!",

            ConfirmPassword =
                "NewSecurePassword123!"
        };

    var firstResponse =
        await _client.PostAsJsonAsync(
            "/api/auth/reset-password",
            request);

    Assert.Equal(
        HttpStatusCode.OK,
        firstResponse.StatusCode);

    // Act
    var secondResponse =
        await _client.PostAsJsonAsync(
            "/api/auth/reset-password",
            request);

    // Assert
    Assert.Equal(
        HttpStatusCode.BadRequest,
        secondResponse.StatusCode);
}

private async Task<AuthResponse> RegisterUserAsync(
    string password =
        "SecurePassword123!")
{
    var response =
        await _client.PostAsJsonAsync(
            "/api/auth/register",
            new RegisterRequest
            {
                FullName =
                    "Password Reset User",

                Email =
                    "password-reset@test.com",

                Password =
                    password,

                Role =
                    "Candidate"
            });

    Assert.Equal(
        HttpStatusCode.OK,
        response.StatusCode);

    var result =
        await response.Content
            .ReadFromJsonAsync<AuthResponse>();

    Assert.NotNull(result);

    return result;
}

private static string ExtractResetToken(
    string emailBody)
{
    const string marker =
        "?token=";

    var markerIndex =
        emailBody.IndexOf(
            marker,
            StringComparison.Ordinal);

    Assert.True(
        markerIndex >= 0,
        "Der Reset-Link enthält keinen Token.");

    var tokenStart =
        markerIndex + marker.Length;

    var remainingText =
        emailBody[tokenStart..];

    var tokenEnd =
        remainingText.IndexOfAny(
            new[]
            {
                '\r',
                '\n',
                ' ',
                '<'
            });

    var encodedToken =
        tokenEnd >= 0
            ? remainingText[..tokenEnd]
            : remainingText;

    return Uri.UnescapeDataString(
        encodedToken.Trim());
}

private static string HashToken(
    string token)
{
    var bytes =
        Encoding.UTF8.GetBytes(token);

    var hash =
        SHA256.HashData(bytes);

    return Convert.ToHexString(hash);
}

}
