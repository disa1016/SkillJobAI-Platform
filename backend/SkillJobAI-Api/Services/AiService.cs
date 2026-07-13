using System.Text;
using Microsoft.EntityFrameworkCore;
using SkillJobAI.Api.Data;
using SkillJobAI.Api.Models;
using SkillJobAI.Api.Models.Responses;
using UglyToad.PdfPig;

namespace SkillJobAI.Api.Services;

public class AiService : IAiService
{
    private readonly AppDbContext _context;

    public AiService(AppDbContext context)
    {
        _context = context;
    }

    public AnalyzeCvResponse AnalyzeCv(AnalyzeCvRequest request)
    {
        return AnalyzeCvText(request.CvText);
    }

    public async Task<AnalyzeCvPdfResponse?> AnalyzeCvPdfAsync(IFormFile file)
    {
        if (file == null || file.Length == 0)
            return null;

        var cvText = new StringBuilder();

        await using var stream = file.OpenReadStream();

        using var document = PdfDocument.Open(stream);

        foreach (var page in document.GetPages())
        {
            cvText.AppendLine(page.Text);
        }

        if (string.IsNullOrWhiteSpace(cvText.ToString()))
            return null;

        var result = AnalyzeCvText(cvText.ToString());

        return new AnalyzeCvPdfResponse
        {
            FileName = file.FileName,
            ExtractedText = cvText.ToString(),
            Score = result.Score,
            Skills = result.Skills,
            SkillCategories = result.SkillCategories,
            Suggestions = result.Suggestions
        };
    }

    public JobMatchResponse JobMatch(JobMatchRequest request)
    {
        var result = CalculateJobMatch(request.CvText, request.JobDescription);

        return new JobMatchResponse
        {
            MatchScore = result.MatchScore,
            MatchedSkills = result.MatchedSkills,
            MissingSkills = result.MissingSkills,
            Recommendation = GetRecommendation(result.MatchScore)
        };
    }

    public async Task<List<JobRecommendationResponse>> JobRecommendationsAsync(JobRecommendationsRequest request)
    {
        var jobs = await _context.Jobs
            .Include(j => j.Company)
            .ToListAsync();

        return jobs
            .Select(job =>
            {
                var companyName = job.Company != null ? job.Company.Name : null;

                var jobDescription =
                    $"{job.Title} {job.Description} {companyName} {job.Location}";

                var result = CalculateJobMatch(request.CvText, jobDescription);

                return new JobRecommendationResponse
                {
                    JobId = job.Id,
                    Title = job.Title,
                    Company = companyName,
                    Location = job.Location,
                    Salary = job.Salary,
                    Description = job.Description,
                    MatchScore = result.MatchScore,
                    MatchedSkills = result.MatchedSkills,
                    MissingSkills = result.MissingSkills,
                    Recommendation = GetRecommendation(result.MatchScore)
                };
            })
            .OrderByDescending(r => r.MatchScore)
            .ToList();
    }

    public CoverLetterResponse GenerateCoverLetter(CoverLetterRequest request)
    {
        var letter = $@"
Sehr geehrte Damen und Herren,

mit großem Interesse habe ich Ihre Stellenausschreibung gelesen.

Aufgrund meiner Kenntnisse und Erfahrungen bin ich überzeugt,
einen wertvollen Beitrag zu Ihrem Unternehmen leisten zu können.

Mein Profil:
{request.CvSummary}

Besonders interessiert mich die Position als:

{request.JobTitle}

bei

{request.Company}.

Ich freue mich darauf, meine Fähigkeiten in Ihrem Team einzubringen
und mich persönlich bei Ihnen vorzustellen.

Mit freundlichen Grüßen

{request.FullName}
";

        return new CoverLetterResponse
        {
            CoverLetter = letter.Trim()
        };
    }

    private static AnalyzeCvResponse AnalyzeCvText(string cvText)
    {
        var text = cvText.ToLower();

        var skillCategories = new List<SkillCategoryResponse>
        {
            AnalyzeCategory(text, "Backend", new List<string>
            {
                "C#",
                "ASP.NET Core",
                ".NET",
                "Node.js",
                "REST API",
                "Microservices"
            }),
            AnalyzeCategory(text, "Frontend", new List<string>
            {
                "Vue.js",
                "React",
                "Angular",
                "JavaScript",
                "TypeScript",
                "HTML",
                "CSS",
                "Bootstrap"
            }),
            AnalyzeCategory(text, "Database", new List<string>
            {
                "SQL",
                "PostgreSQL",
                "MySQL",
                "MongoDB",
                "Entity Framework"
            }),
            AnalyzeCategory(text, "DevOps", new List<string>
            {
                "Git",
                "GitHub",
                "Docker",
                "Kubernetes",
                "CI/CD",
                "Linux"
            }),
            AnalyzeCategory(text, "Cloud", new List<string>
            {
                "Azure",
                "AWS",
                "Google Cloud"
            })
        };

        var skills = skillCategories
            .SelectMany(c => c.MatchedSkills)
            .Distinct()
            .ToList();

        var totalSkills = skillCategories
            .Sum(c => c.MatchedSkills.Count + c.MissingSkills.Count);

        var matchedSkillsCount = skillCategories
            .Sum(c => c.MatchedSkills.Count);

        var score = totalSkills == 0
            ? 0
            : (int)Math.Round((double)matchedSkillsCount / totalSkills * 100);

        var suggestions = new List<string>();

        foreach (var category in skillCategories)
        {
            if (category.MatchedSkills.Count == 0)
            {
                suggestions.Add(
                    $"Ergänze Kenntnisse im Bereich {category.Name}, z.B. {string.Join(", ", category.MissingSkills.Take(3))}.");
            }
            else if (category.MissingSkills.Count > 0)
            {
                suggestions.Add(
                    $"Im Bereich {category.Name} kannst du noch {string.Join(", ", category.MissingSkills.Take(3))} ergänzen.");
            }
        }

        if (!text.Contains("github"))
            suggestions.Add("Füge einen GitHub-Link oder Projekt-Repositorys hinzu.");

        if (!text.Contains("linkedin"))
            suggestions.Add("Füge dein LinkedIn-Profil hinzu.");

        if (!text.Contains("projekt") && !text.Contains("project"))
            suggestions.Add("Beschreibe konkrete Projekte mit Technologien und Ergebnissen.");

        if (suggestions.Count == 0)
            suggestions.Add("Dein Lebenslauf enthält bereits viele relevante technische Informationen.");

        return new AnalyzeCvResponse
        {
            Score = score,
            Skills = skills,
            SkillCategories = skillCategories,
            Suggestions = suggestions
        };
    }

    private static SkillCategoryResponse AnalyzeCategory(
        string text,
        string categoryName,
        List<string> skills)
    {
        var matchedSkills = new List<string>();
        var missingSkills = new List<string>();

        foreach (var skill in skills)
        {
            if (ContainsSkill(text, skill))
                matchedSkills.Add(skill);
            else
                missingSkills.Add(skill);
        }

        return new SkillCategoryResponse
        {
            Name = categoryName,
            MatchedSkills = matchedSkills,
            MissingSkills = missingSkills
        };
    }

    private static bool ContainsSkill(string text, string skill)
    {
        var normalizedSkill = skill.ToLower();

        return normalizedSkill switch
        {
            "c#" => text.Contains("c#") || text.Contains("csharp"),
            "asp.net core" => text.Contains("asp.net") || text.Contains("asp net"),
            ".net" => text.Contains(".net") || text.Contains("dotnet"),
            "vue.js" => text.Contains("vue") || text.Contains("vue.js"),
            "node.js" => text.Contains("node") || text.Contains("node.js"),
            "rest api" => text.Contains("rest") || text.Contains("api"),
            "entity framework" => text.Contains("entity framework") || text.Contains("ef core"),
            "ci/cd" => text.Contains("ci/cd") || text.Contains("pipeline"),
            "google cloud" => text.Contains("google cloud") || text.Contains("gcp"),
            _ => text.Contains(normalizedSkill)
        };
    }

    private static JobMatchResponse CalculateJobMatch(string cvTextInput, string jobTextInput)
    {
        var cvText = cvTextInput.ToLower();
        var jobText = jobTextInput.ToLower();

        var skills = new List<string>
        {
            "c#",
            "asp.net",
            ".net",
            "vue",
            "javascript",
            "sql",
            "postgresql",
            "git",
            "github",
            "docker",
            "azure"
        };

        var matchedSkills = new List<string>();
        var missingSkills = new List<string>();

        foreach (var skill in skills)
        {
            var skillInJob = jobText.Contains(skill);
            var skillInCv = cvText.Contains(skill);

            if (skillInJob && skillInCv)
                matchedSkills.Add(skill);

            if (skillInJob && !skillInCv)
                missingSkills.Add(skill);
        }

        var totalRequiredSkills = matchedSkills.Count + missingSkills.Count;

        var matchScore = totalRequiredSkills == 0
            ? 0
            : (int)Math.Round((double)matchedSkills.Count / totalRequiredSkills * 100);

        return new JobMatchResponse
        {
            MatchScore = matchScore,
            MatchedSkills = matchedSkills,
            MissingSkills = missingSkills
        };
    }

    private static string GetRecommendation(int matchScore)
    {
        return matchScore >= 80
            ? "Sehr guter Match. Du kannst dich auf diese Stelle bewerben."
            : matchScore >= 50
                ? "Guter Anfang. Ergänze noch fehlende Skills."
                : "Der Match ist noch niedrig. Verbessere deinen Lebenslauf gezielt.";
    }
}