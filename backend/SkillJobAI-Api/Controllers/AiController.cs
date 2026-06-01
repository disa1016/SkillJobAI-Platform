using System.Text;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SkillJobAI.Api.Data;
using UglyToad.PdfPig;

namespace SkillJobAI.Api.Controllers;

[ApiController]
[Route("api/ai")]
public class AiController : ControllerBase
{
    private readonly AppDbContext _context;

    public AiController(AppDbContext context)
    {
        _context = context;
    }

    [Authorize]
    [HttpPost("analyze-cv")]
    public IActionResult AnalyzeCv(AnalyzeCvRequest request)
    {
        return Ok(AnalyzeCvText(request.CvText));
    }

    [Authorize]
    [HttpPost("analyze-cv-pdf")]
    public async Task<IActionResult> AnalyzeCvPdf(IFormFile file)
    {
        if (file == null || file.Length == 0)
        {
            return BadRequest(new
            {
                message = "Bitte lade eine PDF-Datei hoch."
            });
        }

        if (!file.FileName.EndsWith(".pdf", StringComparison.OrdinalIgnoreCase))
        {
            return BadRequest(new
            {
                message = "Nur PDF-Dateien sind erlaubt."
            });
        }

        var cvText = new StringBuilder();

        await using var stream = file.OpenReadStream();

        using var document = PdfDocument.Open(stream);

        foreach (var page in document.GetPages())
        {
            cvText.AppendLine(page.Text);
        }

        if (string.IsNullOrWhiteSpace(cvText.ToString()))
        {
            return BadRequest(new
            {
                message = "Aus dieser PDF konnte kein Text gelesen werden."
            });
        }

        var result = AnalyzeCvText(cvText.ToString());

        return Ok(new
        {
            fileName = file.FileName,
            extractedText = cvText.ToString(),
            result.Score,
            result.Skills,
            result.SkillCategories,
            result.Suggestions
        });
    }

    [Authorize]
    [HttpPost("job-match")]
    public IActionResult JobMatch(JobMatchRequest request)
    {
        var result = CalculateJobMatch(
            request.CvText,
            request.JobDescription
        );

        return Ok(new
        {
            matchScore = result.MatchScore,
            matchedSkills = result.MatchedSkills,
            missingSkills = result.MissingSkills,
            recommendation = GetRecommendation(result.MatchScore)
        });
    }

    [Authorize]
    [HttpPost("job-recommendations")]
    public async Task<IActionResult> JobRecommendations(JobRecommendationsRequest request)
    {
        var jobs = await _context.Jobs.ToListAsync();

        var recommendations = jobs
            .Select(job =>
            {
                var jobDescription =
                    $"{job.Title} {job.Description} {job.Company} {job.Location}";

                var result = CalculateJobMatch(
                    request.CvText,
                    jobDescription
                );

                return new
                {
                    jobId = job.Id,
                    title = job.Title,
                    company = job.Company,
                    location = job.Location,
                    salary = job.Salary,
                    description = job.Description,
                    matchScore = result.MatchScore,
                    matchedSkills = result.MatchedSkills,
                    missingSkills = result.MissingSkills,
                    recommendation = GetRecommendation(result.MatchScore)
                };
            })
            .OrderByDescending(r => r.matchScore)
            .ToList();

        return Ok(recommendations);
    }

    private static AnalyzeCvResult AnalyzeCvText(string cvText)
    {
        var text = cvText.ToLower();

        var skillCategories = new List<SkillCategoryResult>
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

        return new AnalyzeCvResult
        {
            Score = score,
            Skills = skills,
            SkillCategories = skillCategories,
            Suggestions = suggestions
        };
    }

    private static SkillCategoryResult AnalyzeCategory(
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

        return new SkillCategoryResult
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

    private static JobMatchResult CalculateJobMatch(
        string cvTextInput,
        string jobTextInput)
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

        var totalRequiredSkills =
            matchedSkills.Count + missingSkills.Count;

        var matchScore = totalRequiredSkills == 0
            ? 0
            : (int)Math.Round(
                (double)matchedSkills.Count /
                totalRequiredSkills * 100);

        return new JobMatchResult
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
[Authorize]
[HttpPost("generate-cover-letter")]
public IActionResult GenerateCoverLetter(CoverLetterRequest request)
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

    return Ok(new
    {
        coverLetter = letter.Trim()
    });
}
}

public class AnalyzeCvRequest
{
    public string CvText { get; set; } = "";
}

public class AnalyzeCvResult
{
    public int Score { get; set; }

    public List<string> Skills { get; set; } = new();

    public List<SkillCategoryResult> SkillCategories { get; set; } = new();

    public List<string> Suggestions { get; set; } = new();
}

public class SkillCategoryResult
{
    public string Name { get; set; } = "";

    public List<string> MatchedSkills { get; set; } = new();

    public List<string> MissingSkills { get; set; } = new();
}

public class JobMatchRequest
{
    public string CvText { get; set; } = "";
    public string JobDescription { get; set; } = "";
}

public class JobRecommendationsRequest
{
    public string CvText { get; set; } = "";
}

public class JobMatchResult
{
    public int MatchScore { get; set; }

    public List<string> MatchedSkills { get; set; } = new();

    public List<string> MissingSkills { get; set; } = new();
}

public class CoverLetterRequest
{
    public string FullName { get; set; } = "";

    public string Company { get; set; } = "";

    public string JobTitle { get; set; } = "";

    public string CvSummary { get; set; } = "";
}