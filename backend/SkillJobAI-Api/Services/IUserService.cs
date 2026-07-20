using Microsoft.AspNetCore.Http;
using SkillJobAI.Api.Models;
using SkillJobAI.Api.Models.Responses;

namespace SkillJobAI.Api.Services;

public interface IUserService
{
    Task<UserResponse?> GetProfileAsync(
        int userId
    );

    Task<UserResponse?> UpdateProfileAsync(
        int userId,
        UpdateProfileRequest request
    );

    Task<(
        bool Success,
        string? ErrorMessage,
        string? CvUrl
    )> UploadCvAsync(
        int userId,
        IFormFile file
    );

    Task<(
        string FilePath,
        string DownloadName
    )?> GetCvDownloadAsync(
        int userId
    );

    Task<bool> DeleteCvAsync(
        int userId
    );

    Task<(
        bool Success,
        string? ErrorMessage,
        string? ProfileImageUrl
    )> UploadProfileImageAsync(
        int userId,
        IFormFile file
    );

    Task<bool> DeleteProfileImageAsync(
        int userId
    );

    Task<UserResponse?> UpdateUserRoleAsync(
        int id,
        UpdateUserRoleRequest request
    );

    Task<(
        bool Success,
        string? ErrorMessage,
        byte[]? ZipBytes,
        string? DownloadName
    )> ExportPersonalDataAsync(
        int userId
    );

    Task<(
        bool Success,
        string? ErrorMessage
    )> DeleteAccountAsync(
        int userId,
        DeleteAccountRequest request
    );
}