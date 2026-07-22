using System.Net;
using System.Net.Http.Json;
using Microsoft.EntityFrameworkCore;
using SkillJobAI.Api.Models;
using SkillJobAI.Api.Models.Responses;

namespace SkillJobAI.Api.Tests.Integration;

public class AuthEndpointsTests
    : IClassFixture<CustomWebApplicationFactory>
{
    private const string RefreshTokenCookieName =
        "skilljobai_refresh_token";

    private readonly CustomWebApplicationFactory
        _factory;

    private readonly HttpClient
        _client;

    public AuthEndpointsTests(
        CustomWebApplicationFactory factory)
    {
        _factory = factory;
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task Register_ShouldReturnOk_WhenRequestIsValid()
    {
        // Arrange
        await _factory.ResetDatabaseAsync();

        var request =
            CreateRegisterRequest();

        // Act
        var response =
            await _client.PostAsJsonAsync(
                "/api/auth/register",
                request);

        // Assert
        Assert.Equal(
            HttpStatusCode.OK,
            response.StatusCode);

        var result =
            await response.Content
                .ReadFromJsonAsync<AuthResponse>();

        Assert.NotNull(result);

        Assert.Equal(
            "User registered successfully",
            result.Message);

        Assert.False(
            string.IsNullOrWhiteSpace(
                result.Token));

        Assert.True(
            string.IsNullOrEmpty(
                result.RefreshToken));

        var rawRefreshToken =
            GetRefreshTokenFromResponse(
                response);

        Assert.True(
            result.TokenExpiresAt >
            DateTime.UtcNow);

        Assert.Equal(
            request.FullName,
            result.User.FullName);

        Assert.Equal(
            request.Email,
            result.User.Email);

        Assert.Equal(
            "Candidate",
            result.User.Role);

        await _factory.ExecuteDbContextAsync(
            async context =>
            {
                var user =
                    await context.Users
                        .SingleAsync();

                Assert.Equal(
                    request.FullName,
                    user.FullName);

                Assert.Equal(
                    request.Email,
                    user.Email);

                Assert.Equal(
                    "Candidate",
                    user.Role);

                Assert.NotEqual(
                    request.Password,
                    user.PasswordHash);

                var refreshToken =
                    await context.RefreshTokens
                        .SingleAsync();

                Assert.Equal(
                    user.Id,
                    refreshToken.UserId);

                Assert.NotEqual(
                    rawRefreshToken,
                    refreshToken.TokenHash);

                Assert.False(
                    string.IsNullOrWhiteSpace(
                        refreshToken.TokenHash));

                Assert.Null(
                    refreshToken.RevokedAt);
            });
    }

    [Fact]
    public async Task Register_ShouldNormalizeEmailAndFullName()
    {
        // Arrange
        await _factory.ResetDatabaseAsync();

        var request =
            new RegisterRequest
            {
                FullName = "  Test Candidate  ",
                Email = "  CANDIDATE@TEST.COM  ",
                Password = "SecurePassword123!",
                Role = "Candidate"
            };

        // Act
        var response =
            await _client.PostAsJsonAsync(
                "/api/auth/register",
                request);

        // Assert
        Assert.Equal(
            HttpStatusCode.OK,
            response.StatusCode);

        var result =
            await response.Content
                .ReadFromJsonAsync<AuthResponse>();

        Assert.NotNull(result);

        Assert.Equal(
            "Test Candidate",
            result.User.FullName);

        Assert.Equal(
            "candidate@test.com",
            result.User.Email);

        await _factory.ExecuteDbContextAsync(
            async context =>
            {
                var user =
                    await context.Users
                        .SingleAsync();

                Assert.Equal(
                    "Test Candidate",
                    user.FullName);

                Assert.Equal(
                    "candidate@test.com",
                    user.Email);
            });
    }

    [Theory]
    [InlineData("Admin")]
    [InlineData("Recruiter")]
    public async Task Register_ShouldAlwaysAssignCandidateRole(
        string requestedRole)
    {
        // Arrange
        await _factory.ResetDatabaseAsync();

        var request =
            CreateRegisterRequest();

        request.Role =
            requestedRole;

        // Act
        var response =
            await _client.PostAsJsonAsync(
                "/api/auth/register",
                request);

        // Assert
        Assert.Equal(
            HttpStatusCode.OK,
            response.StatusCode);

        var result =
            await response.Content
                .ReadFromJsonAsync<AuthResponse>();

        Assert.NotNull(result);

        Assert.Equal(
            "Candidate",
            result.User.Role);

        await _factory.ExecuteDbContextAsync(
            async context =>
            {
                var user =
                    await context.Users
                        .SingleAsync();

                Assert.Equal(
                    "Candidate",
                    user.Role);
            });
    }

    [Fact]
    public async Task Register_ShouldReturnBadRequest_WhenEmailAlreadyExists()
    {
        // Arrange
        await _factory.ResetDatabaseAsync();

        var request =
            CreateRegisterRequest();

        var firstResponse =
            await _client.PostAsJsonAsync(
                "/api/auth/register",
                request);

        Assert.Equal(
            HttpStatusCode.OK,
            firstResponse.StatusCode);

        // Act
        var secondResponse =
            await _client.PostAsJsonAsync(
                "/api/auth/register",
                request);

        // Assert
        Assert.Equal(
            HttpStatusCode.BadRequest,
            secondResponse.StatusCode);

        var result =
            await secondResponse.Content
                .ReadFromJsonAsync<MessageResponse>();

        Assert.NotNull(result);

        Assert.Equal(
            "Diese E-Mail ist bereits registriert.",
            result.Message);

        await _factory.ExecuteDbContextAsync(
            async context =>
            {
                Assert.Equal(
                    1,
                    await context.Users.CountAsync());
            });
    }

    [Fact]
    public async Task Register_ShouldReturnBadRequest_WhenEmailIsInvalid()
    {
        // Arrange
        await _factory.ResetDatabaseAsync();

        var request =
            CreateRegisterRequest();

        request.Email =
            "not-an-email";

        // Act
        var response =
            await _client.PostAsJsonAsync(
                "/api/auth/register",
                request);

        // Assert
        Assert.Equal(
            HttpStatusCode.BadRequest,
            response.StatusCode);

        await _factory.ExecuteDbContextAsync(
            async context =>
            {
                Assert.Empty(
                    await context.Users
                        .ToListAsync());
            });
    }

    [Fact]
    public async Task Register_ShouldReturnBadRequest_WhenPasswordIsTooShort()
    {
        // Arrange
        await _factory.ResetDatabaseAsync();

        var request =
            CreateRegisterRequest();

        request.Password =
            "12345";

        // Act
        var response =
            await _client.PostAsJsonAsync(
                "/api/auth/register",
                request);

        // Assert
        Assert.Equal(
            HttpStatusCode.BadRequest,
            response.StatusCode);
    }

    [Fact]
    public async Task Login_ShouldReturnOk_WhenCredentialsAreCorrect()
    {
        // Arrange
        await _factory.ResetDatabaseAsync();

        var registerRequest =
            CreateRegisterRequest();

        var registrationResponse =
            await _client.PostAsJsonAsync(
                "/api/auth/register",
                registerRequest);

        Assert.Equal(
            HttpStatusCode.OK,
            registrationResponse.StatusCode);

        var loginRequest =
            new LoginRequest
            {
                Email =
                    registerRequest.Email,
                Password =
                    registerRequest.Password
            };

        // Act
        var response =
            await _client.PostAsJsonAsync(
                "/api/auth/login",
                loginRequest);

        // Assert
        Assert.Equal(
            HttpStatusCode.OK,
            response.StatusCode);

        var result =
            await response.Content
                .ReadFromJsonAsync<AuthResponse>();

        Assert.NotNull(result);

        Assert.Equal(
            "Login successful",
            result.Message);

        Assert.False(
            string.IsNullOrWhiteSpace(
                result.Token));

        Assert.True(
            string.IsNullOrEmpty(
                result.RefreshToken));

        var refreshToken =
            GetRefreshTokenFromResponse(
                response);

        Assert.False(
            string.IsNullOrWhiteSpace(
                refreshToken));

        Assert.Equal(
            registerRequest.Email,
            result.User.Email);

        await _factory.ExecuteDbContextAsync(
            async context =>
            {
                Assert.Equal(
                    2,
                    await context.RefreshTokens
                        .CountAsync());
            });
    }

    [Fact]
    public async Task Login_ShouldNormalizeEmail()
    {
        // Arrange
        await _factory.ResetDatabaseAsync();

        var registerRequest =
            CreateRegisterRequest();

        await RegisterUserAsync(
            registerRequest);

        var loginRequest =
            new LoginRequest
            {
                Email =
                    $"  {registerRequest.Email.ToUpperInvariant()}  ",
                Password =
                    registerRequest.Password
            };

        // Act
        var response =
            await _client.PostAsJsonAsync(
                "/api/auth/login",
                loginRequest);

        // Assert
        Assert.Equal(
            HttpStatusCode.OK,
            response.StatusCode);
    }

    [Fact]
    public async Task Login_ShouldReturnUnauthorized_WhenPasswordIsIncorrect()
    {
        // Arrange
        await _factory.ResetDatabaseAsync();

        var registerRequest =
            CreateRegisterRequest();

        await RegisterUserAsync(
            registerRequest);

        var loginRequest =
            new LoginRequest
            {
                Email =
                    registerRequest.Email,
                Password =
                    "WrongPassword123!"
            };

        // Act
        var response =
            await _client.PostAsJsonAsync(
                "/api/auth/login",
                loginRequest);

        // Assert
        Assert.Equal(
            HttpStatusCode.Unauthorized,
            response.StatusCode);

        var result =
            await response.Content
                .ReadFromJsonAsync<MessageResponse>();

        Assert.NotNull(result);

        Assert.Equal(
            "E-Mail oder Passwort ist falsch.",
            result.Message);
    }

    [Fact]
    public async Task Login_ShouldReturnUnauthorized_WhenUserDoesNotExist()
    {
        // Arrange
        await _factory.ResetDatabaseAsync();

        var request =
            new LoginRequest
            {
                Email =
                    "unknown@test.com",
                Password =
                    "SecurePassword123!"
            };

        // Act
        var response =
            await _client.PostAsJsonAsync(
                "/api/auth/login",
                request);

        // Assert
        Assert.Equal(
            HttpStatusCode.Unauthorized,
            response.StatusCode);
    }

    [Fact]
    public async Task RefreshToken_ShouldRotateToken_WhenTokenIsValid()
    {
        // Arrange
        await _factory.ResetDatabaseAsync();

        var registration =
            await RegisterUserWithRefreshTokenAsync(
                CreateRegisterRequest());

        var oldRefreshToken =
            registration.RefreshToken;

        // Act
        var response =
            await PostWithRefreshTokenCookieAsync(
                "/api/auth/refresh",
                oldRefreshToken);

        // Assert
        Assert.Equal(
            HttpStatusCode.OK,
            response.StatusCode);

        var result =
            await response.Content
                .ReadFromJsonAsync<AuthResponse>();

        Assert.NotNull(result);

        Assert.Equal(
            "Token refreshed successfully",
            result.Message);

        Assert.True(
            string.IsNullOrEmpty(
                result.RefreshToken));

        Assert.False(
            string.IsNullOrWhiteSpace(
                result.Token));

        var newRefreshToken =
            GetRefreshTokenFromResponse(
                response);

        Assert.NotEqual(
            oldRefreshToken,
            newRefreshToken);

        await _factory.ExecuteDbContextAsync(
            async context =>
            {
                var tokens =
                    await context.RefreshTokens
                        .OrderBy(token => token.Id)
                        .ToListAsync();

                Assert.Equal(
                    2,
                    tokens.Count);

                var oldStoredToken =
                    tokens[0];

                var newStoredToken =
                    tokens[1];

                Assert.NotNull(
                    oldStoredToken.RevokedAt);

                Assert.Equal(
                    newStoredToken.TokenHash,
                    oldStoredToken
                        .ReplacedByTokenHash);

                Assert.Null(
                    newStoredToken.RevokedAt);
            });
    }

    [Fact]
    public async Task RefreshToken_ShouldReturnUnauthorized_WhenTokenIsUnknown()
    {
        // Arrange
        await _factory.ResetDatabaseAsync();

        // Act
        var response =
            await PostWithRefreshTokenCookieAsync(
                "/api/auth/refresh",
                "unknown-refresh-token");

        // Assert
        Assert.Equal(
            HttpStatusCode.Unauthorized,
            response.StatusCode);
    }

    [Fact]
    public async Task RefreshToken_ShouldRevokeAllSessions_WhenOldTokenIsReused()
    {
        // Arrange
        await _factory.ResetDatabaseAsync();

        var registration =
            await RegisterUserWithRefreshTokenAsync(
                CreateRegisterRequest());

        var oldRefreshToken =
            registration.RefreshToken;

        var firstRefreshResponse =
            await PostWithRefreshTokenCookieAsync(
                "/api/auth/refresh",
                oldRefreshToken);

        Assert.Equal(
            HttpStatusCode.OK,
            firstRefreshResponse.StatusCode);

        var firstRefreshResult =
            await firstRefreshResponse.Content
                .ReadFromJsonAsync<AuthResponse>();

        Assert.NotNull(
            firstRefreshResult);

        var newRefreshToken =
            GetRefreshTokenFromResponse(
                firstRefreshResponse);

        // Act: Alten Token erneut verwenden
        var reuseResponse =
            await PostWithRefreshTokenCookieAsync(
                "/api/auth/refresh",
                oldRefreshToken);

        // Assert
        Assert.Equal(
            HttpStatusCode.Unauthorized,
            reuseResponse.StatusCode);

        await _factory.ExecuteDbContextAsync(
            async context =>
            {
                var tokens =
                    await context.RefreshTokens
                        .ToListAsync();

                Assert.Equal(
                    2,
                    tokens.Count);

                Assert.All(
                    tokens,
                    token =>
                        Assert.NotNull(
                            token.RevokedAt));
            });

        // Auch der neue Token wurde durch die
        // Reuse-Detection widerrufen.
        var newTokenResponse =
            await PostWithRefreshTokenCookieAsync(
                "/api/auth/refresh",
                newRefreshToken);

        Assert.Equal(
            HttpStatusCode.Unauthorized,
            newTokenResponse.StatusCode);
    }

    [Fact]
    public async Task Logout_ShouldReturnOk_WhenRefreshTokenIsValid()
    {
        // Arrange
        await _factory.ResetDatabaseAsync();

        var registration =
            await RegisterUserWithRefreshTokenAsync(
                CreateRegisterRequest());

        // Act
        var response =
            await PostWithRefreshTokenCookieAsync(
                "/api/auth/logout",
                registration.RefreshToken);

        // Assert
        Assert.Equal(
            HttpStatusCode.OK,
            response.StatusCode);

        var result =
            await response.Content
                .ReadFromJsonAsync<MessageResponse>();

        Assert.NotNull(result);

        Assert.Equal(
            "Logout erfolgreich.",
            result.Message);

        await _factory.ExecuteDbContextAsync(
            async context =>
            {
                var token =
                    await context.RefreshTokens
                        .SingleAsync();

                Assert.NotNull(
                    token.RevokedAt);
            });
    }

    [Fact]
    public async Task Logout_ShouldReturnOk_WhenTokenWasAlreadyRevoked()
    {
        // Arrange
        await _factory.ResetDatabaseAsync();

        var registration =
            await RegisterUserWithRefreshTokenAsync(
                CreateRegisterRequest());

        var firstResponse =
            await PostWithRefreshTokenCookieAsync(
                "/api/auth/logout",
                registration.RefreshToken);

        Assert.Equal(
            HttpStatusCode.OK,
            firstResponse.StatusCode);

        // Act
        var secondResponse =
            await PostWithRefreshTokenCookieAsync(
                "/api/auth/logout",
                registration.RefreshToken);

        // Assert
        Assert.Equal(
            HttpStatusCode.OK,
            secondResponse.StatusCode);

        var result =
            await secondResponse.Content
                .ReadFromJsonAsync<MessageResponse>();

        Assert.NotNull(result);

        Assert.Equal(
            "Logout erfolgreich.",
            result.Message);
    }

    [Fact]
    public async Task Logout_ShouldReturnOk_WhenTokenIsUnknown()
    {
        // Arrange
        await _factory.ResetDatabaseAsync();

        // Act
        var response =
            await PostWithRefreshTokenCookieAsync(
                "/api/auth/logout",
                "unknown-refresh-token");

        // Assert
        Assert.Equal(
            HttpStatusCode.OK,
            response.StatusCode);

        var result =
            await response.Content
                .ReadFromJsonAsync<MessageResponse>();

        Assert.NotNull(result);

        Assert.Equal(
            "Logout erfolgreich.",
            result.Message);
    }

    [Fact]
    public async Task LoggedOutRefreshToken_ShouldNotBeUsable()
    {
        // Arrange
        await _factory.ResetDatabaseAsync();

        var registration =
            await RegisterUserWithRefreshTokenAsync(
                CreateRegisterRequest());

        var logoutResponse =
            await PostWithRefreshTokenCookieAsync(
                "/api/auth/logout",
                registration.RefreshToken);

        Assert.Equal(
            HttpStatusCode.OK,
            logoutResponse.StatusCode);

        // Act
        var refreshResponse =
            await PostWithRefreshTokenCookieAsync(
                "/api/auth/refresh",
                registration.RefreshToken);

        // Assert
        Assert.Equal(
            HttpStatusCode.Unauthorized,
            refreshResponse.StatusCode);
    }

    private static string GetRefreshTokenFromResponse(
        HttpResponseMessage response)
    {
        var cookieHeaders =
            response.Headers.TryGetValues(
                "Set-Cookie",
                out var values)
                ? values
                : [];

        var refreshTokenCookie =
            cookieHeaders.FirstOrDefault(
                header =>
                    header.StartsWith(
                        $"{RefreshTokenCookieName}=",
                        StringComparison.OrdinalIgnoreCase));

        Assert.False(
            string.IsNullOrWhiteSpace(
                refreshTokenCookie));

        var cookieValue =
            refreshTokenCookie!
                .Split(
                    ';',
                    StringSplitOptions.RemoveEmptyEntries)[0];

        var separatorIndex =
            cookieValue.IndexOf('=');

        Assert.True(
            separatorIndex >= 0);

        var refreshToken =
            cookieValue[
                (separatorIndex + 1)..];

        Assert.False(
            string.IsNullOrWhiteSpace(
                refreshToken));

        return refreshToken;
    }

    private async Task<HttpResponseMessage>
        PostWithRefreshTokenCookieAsync(
            string requestUri,
            string refreshToken)
    {
        using var request =
            new HttpRequestMessage(
                HttpMethod.Post,
                requestUri);

        request.Headers.TryAddWithoutValidation(
            "Cookie",
            $"{RefreshTokenCookieName}={refreshToken}");

        return await _client.SendAsync(request);
    }

    private async Task<(
        AuthResponse Result,
        string RefreshToken
    )> RegisterUserWithRefreshTokenAsync(
        RegisterRequest request)
    {
        var response =
            await _client.PostAsJsonAsync(
                "/api/auth/register",
                request);

        Assert.Equal(
            HttpStatusCode.OK,
            response.StatusCode);

        var result =
            await response.Content
                .ReadFromJsonAsync<AuthResponse>();

        Assert.NotNull(result);

        var refreshToken =
            GetRefreshTokenFromResponse(
                response);

        return (
            result,
            refreshToken
        );
    }

    private async Task<AuthResponse> RegisterUserAsync(
        RegisterRequest request)
    {
        var response =
            await _client.PostAsJsonAsync(
                "/api/auth/register",
                request);

        Assert.Equal(
            HttpStatusCode.OK,
            response.StatusCode);

        var result =
            await response.Content
                .ReadFromJsonAsync<AuthResponse>();

        Assert.NotNull(result);

        return result;
    }

    private static RegisterRequest
        CreateRegisterRequest()
    {
        return new RegisterRequest
        {
            FullName = "Test Candidate",
            Email = "candidate@test.com",
            Password = "SecurePassword123!",
            Role = "Candidate"
        };
    }

}