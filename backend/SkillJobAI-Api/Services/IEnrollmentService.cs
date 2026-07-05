using SkillJobAI.Api.Models;
using SkillJobAI.Api.Models.Responses;

namespace SkillJobAI.Api.Services;

public interface IEnrollmentService
{
    Task<(bool Success, string? ErrorMessage, EnrollmentResponse? Enrollment)> EnrollAsync(
        int userId,
        EnrollmentRequest request);

    Task<List<EnrollmentResponse>> GetMyEnrollmentsAsync(int userId);
}