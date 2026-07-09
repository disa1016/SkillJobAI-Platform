using SkillJobAI.Api.Models;
using SkillJobAI.Api.Models.Responses;

namespace SkillJobAI.Api.Services;

public interface IAiService
{
    AnalyzeCvResponse AnalyzeCv(AnalyzeCvRequest request);
    Task<AnalyzeCvPdfResponse?> AnalyzeCvPdfAsync(IFormFile file);
    JobMatchResponse JobMatch(JobMatchRequest request);
    Task<List<JobRecommendationResponse>> JobRecommendationsAsync(JobRecommendationsRequest request);
    CoverLetterResponse GenerateCoverLetter(CoverLetterRequest request);
}