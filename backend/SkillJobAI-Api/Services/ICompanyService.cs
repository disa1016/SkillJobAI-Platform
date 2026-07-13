using SkillJobAI.Api.Models;
using SkillJobAI.Api.Models.Responses;

namespace SkillJobAI.Api.Services;

public interface ICompanyService
{
    Task<PagedResponse<CompanyResponse>> GetCompaniesAsync(
        int page,
        int pageSize,
        string? search);

    Task<CompanyResponse?> GetCompanyByIdAsync(int id);

    Task<CompanyResponse> CreateCompanyAsync(CompanyRequest request);

    Task<CompanyResponse?> UpdateCompanyAsync(int id, CompanyRequest request);

    Task<(bool Success, string? ErrorMessage, string? LogoUrl)> UploadCompanyLogoAsync(
        int id,
        IFormFile file);

    Task<bool> DeleteCompanyAsync(int id);
}