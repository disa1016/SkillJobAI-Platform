using SkillJobAI.Api.Models;
using SkillJobAI.Api.Models.Responses;

namespace SkillJobAI.Api.Services;

public interface IAuthService
{
    Task<AuthResponse?> RegisterAsync(RegisterRequest request);

    Task<AuthResponse?> LoginAsync(LoginRequest request);

    Task<MessageResponse> ForgotPasswordAsync(
        ForgotPasswordRequest request);

    Task<PasswordResetResponse> ResetPasswordAsync(
        ResetPasswordRequest request);
}