using Microsoft.AspNetCore.Http;
using SkillJobAI.Api.Entities;
using SkillJobAI.Api.Models;
using SkillJobAI.Api.Services;
using SkillJobAI.Api.Tests.Helpers;

namespace SkillJobAI.Api.Tests.Services;

public class AiServiceTests
{
    [Fact]
    public void AnalyzeCv_ShouldReturnZeroScore_WhenCvContainsNoTechnicalSkills()
    {
        // Arrange
        using var context =
            TestDbContextFactory.Create();

        var service =
            new AiService(context);

        var request =
            new AnalyzeCvRequest
            {
                CvText =
                    "Ich habe Berufserfahrung im Kundenservice."
            };

        // Act
        var result =
            service.AnalyzeCv(request);

        // Assert
        Assert.Equal(
            0,
            result.Score);

        Assert.Empty(
            result.Skills);

        Assert.Equal(
            5,
            result.SkillCategories.Count);

        Assert.NotEmpty(
            result.Suggestions);
    }

    [Fact]
    public void AnalyzeCv_ShouldDetectExactTechnicalSkills()
    {
        // Arrange
        using var context =
            TestDbContextFactory.Create();

        var service =
            new AiService(context);

        var request =
            new AnalyzeCvRequest
            {
                CvText =
                    """
                    C# ASP.NET Core PostgreSQL
                    TypeScript Docker Azure
                    """
            };

        // Act
        var result =
            service.AnalyzeCv(request);

        // Assert
        Assert.Contains(
            "C#",
            result.Skills);

        Assert.Contains(
            "ASP.NET Core",
            result.Skills);

        Assert.Contains(
            "PostgreSQL",
            result.Skills);

        Assert.Contains(
            "TypeScript",
            result.Skills);

        Assert.Contains(
            "Docker",
            result.Skills);

        Assert.Contains(
            "Azure",
            result.Skills);

        Assert.True(
            result.Score > 0);
    }

    [Theory]
    [InlineData("csharp", "C#")]
    [InlineData("asp net", "ASP.NET Core")]
    [InlineData("dotnet", ".NET")]
    [InlineData("vue", "Vue.js")]
    [InlineData("node", "Node.js")]
    [InlineData("rest", "REST API")]
    [InlineData("ef core", "Entity Framework")]
    [InlineData("pipeline", "CI/CD")]
    [InlineData("gcp", "Google Cloud")]
    public void AnalyzeCv_ShouldDetectSkillAliases(
        string cvText,
        string expectedSkill)
    {
        // Arrange
        using var context =
            TestDbContextFactory.Create();

        var service =
            new AiService(context);

        var request =
            new AnalyzeCvRequest
            {
                CvText = cvText
            };

        // Act
        var result =
            service.AnalyzeCv(request);

        // Assert
        Assert.Contains(
            expectedSkill,
            result.Skills);
    }

    [Fact]
    public void AnalyzeCv_ShouldReturnAllSkillCategories()
    {
        // Arrange
        using var context =
            TestDbContextFactory.Create();

        var service =
            new AiService(context);

        // Act
        var result =
            service.AnalyzeCv(
                new AnalyzeCvRequest
                {
                    CvText = string.Empty
                });

        // Assert
        Assert.Equal(
            5,
            result.SkillCategories.Count);

        Assert.Contains(
            result.SkillCategories,
            category =>
                category.Name == "Backend");

        Assert.Contains(
            result.SkillCategories,
            category =>
                category.Name == "Frontend");

        Assert.Contains(
            result.SkillCategories,
            category =>
                category.Name == "Database");

        Assert.Contains(
            result.SkillCategories,
            category =>
                category.Name == "DevOps");

        Assert.Contains(
            result.SkillCategories,
            category =>
                category.Name == "Cloud");
    }

    [Fact]
    public void AnalyzeCv_ShouldSuggestMissingCategory_WhenCategoryHasNoMatches()
    {
        // Arrange
        using var context =
            TestDbContextFactory.Create();

        var service =
            new AiService(context);

        // Act
        var result =
            service.AnalyzeCv(
                new AnalyzeCvRequest
                {
                    CvText = "C#"
                });

        // Assert
        Assert.Contains(
            result.Suggestions,
            suggestion =>
                suggestion.Contains(
                    "Frontend",
                    StringComparison.Ordinal));

        Assert.Contains(
            result.Suggestions,
            suggestion =>
                suggestion.Contains(
                    "Database",
                    StringComparison.Ordinal));
    }

    [Fact]
    public void AnalyzeCv_ShouldSuggestMissingSkills_WhenCategoryIsPartiallyMatched()
    {
        // Arrange
        using var context =
            TestDbContextFactory.Create();

        var service =
            new AiService(context);

        // Act
        var result =
            service.AnalyzeCv(
                new AnalyzeCvRequest
                {
                    CvText = "C#"
                });

        // Assert
        Assert.Contains(
            result.Suggestions,
            suggestion =>
                suggestion.Contains(
                    "Im Bereich Backend",
                    StringComparison.Ordinal));
    }

    [Fact]
    public void AnalyzeCv_ShouldSuggestGithubLinkedInAndProjects_WhenMissing()
    {
        // Arrange
        using var context =
            TestDbContextFactory.Create();

        var service =
            new AiService(context);

        // Act
        var result =
            service.AnalyzeCv(
                new AnalyzeCvRequest
                {
                    CvText = "C# und SQL"
                });

        // Assert
        Assert.Contains(
            "Füge einen GitHub-Link oder Projekt-Repositorys hinzu.",
            result.Suggestions);

        Assert.Contains(
            "Füge dein LinkedIn-Profil hinzu.",
            result.Suggestions);

        Assert.Contains(
            "Beschreibe konkrete Projekte mit Technologien und Ergebnissen.",
            result.Suggestions);
    }

    [Fact]
    public void AnalyzeCv_ShouldNotSuggestProfileLinksOrProjects_WhenPresent()
    {
        // Arrange
        using var context =
            TestDbContextFactory.Create();

        var service =
            new AiService(context);

        // Act
        var result =
            service.AnalyzeCv(
                new AnalyzeCvRequest
                {
                    CvText =
                        """
                        GitHub LinkedIn
                        Projekt mit C# und Docker
                        """
                });

        // Assert
        Assert.DoesNotContain(
            "Füge einen GitHub-Link oder Projekt-Repositorys hinzu.",
            result.Suggestions);

        Assert.DoesNotContain(
            "Füge dein LinkedIn-Profil hinzu.",
            result.Suggestions);

        Assert.DoesNotContain(
            "Beschreibe konkrete Projekte mit Technologien und Ergebnissen.",
            result.Suggestions);
    }

    [Fact]
    public void JobMatch_ShouldReturnPerfectMatch_WhenAllRequiredSkillsArePresent()
    {
        // Arrange
        using var context =
            TestDbContextFactory.Create();

        var service =
            new AiService(context);

        var request =
            new JobMatchRequest
            {
                CvText =
                    "C# ASP.NET .NET SQL Docker Azure",

                JobDescription =
                    "Gesucht: C# ASP.NET .NET SQL Docker Azure"
            };

        // Act
        var result =
            service.JobMatch(request);

        // Assert
        Assert.Equal(
            100,
            result.MatchScore);

        Assert.Empty(
            result.MissingSkills);

        Assert.Equal(
            "Sehr guter Match. Du kannst dich auf diese Stelle bewerben.",
            result.Recommendation);
    }

    [Fact]
    public void JobMatch_ShouldReturnMediumRecommendation_WhenScoreIsBetween50And79()
    {
        // Arrange
        using var context =
            TestDbContextFactory.Create();

        var service =
            new AiService(context);

        var request =
            new JobMatchRequest
            {
                CvText =
                    "c# sql",

                JobDescription =
                    "c# sql docker"
            };

        // Act
        var result =
            service.JobMatch(request);

        // Assert
        Assert.Equal(
            67,
            result.MatchScore);

        Assert.Contains(
            "docker",
            result.MissingSkills);

        Assert.Equal(
            "Guter Anfang. Ergänze noch fehlende Skills.",
            result.Recommendation);
    }

    [Fact]
    public void JobMatch_ShouldReturnLowRecommendation_WhenScoreIsBelow50()
    {
        // Arrange
        using var context =
            TestDbContextFactory.Create();

        var service =
            new AiService(context);

        var request =
            new JobMatchRequest
            {
                CvText =
                    "c#",

                JobDescription =
                    "c# sql docker azure"
            };

        // Act
        var result =
            service.JobMatch(request);

        // Assert
        Assert.Equal(
            25,
            result.MatchScore);

        Assert.Equal(
            3,
            result.MissingSkills.Count);

        Assert.Equal(
            "Der Match ist noch niedrig. Verbessere deinen Lebenslauf gezielt.",
            result.Recommendation);
    }

    [Fact]
    public void JobMatch_ShouldReturnZero_WhenJobContainsNoKnownSkills()
    {
        // Arrange
        using var context =
            TestDbContextFactory.Create();

        var service =
            new AiService(context);

        // Act
        var result =
            service.JobMatch(
                new JobMatchRequest
                {
                    CvText =
                        "C# Docker",

                    JobDescription =
                        "Kommunikation und Teamarbeit"
                });

        // Assert
        Assert.Equal(
            0,
            result.MatchScore);

        Assert.Empty(
            result.MatchedSkills);

        Assert.Empty(
            result.MissingSkills);

        Assert.Equal(
            "Der Match ist noch niedrig. Verbessere deinen Lebenslauf gezielt.",
            result.Recommendation);
    }

    [Fact]
    public async Task JobRecommendationsAsync_ShouldReturnEmptyList_WhenNoJobsExist()
    {
        // Arrange
        await using var context =
            TestDbContextFactory.Create();

        var service =
            new AiService(context);

        // Act
        var result =
            await service.JobRecommendationsAsync(
                new JobRecommendationsRequest
                {
                    CvText = "C# Docker"
                });

        // Assert
        Assert.Empty(result);
    }

    [Fact]
    public async Task JobRecommendationsAsync_ShouldReturnJobsOrderedByMatchScoreDescending()
    {
        // Arrange
        await using var context =
            TestDbContextFactory.Create();

        var company =
            new Company
            {
                Name = "SkillJob GmbH",
                Location = "Berlin"
            };

        var highMatchJob =
            new Job
            {
                Title = "Backend Developer",
                Description =
                    "C# ASP.NET SQL Docker",
                Location = "Berlin",
                Salary = "80.000 EUR",
                Company = company
            };

        var lowMatchJob =
            new Job
            {
                Title = "Cloud Engineer",
                Description =
                    "Azure Docker PostgreSQL",
                Location = "Hamburg",
                Salary = "75.000 EUR",
                Company = company
            };

        context.Companies.Add(company);

        context.Jobs.AddRange(
            lowMatchJob,
            highMatchJob);

        await context.SaveChangesAsync();

        var service =
            new AiService(context);

        // Act
        var result =
            await service.JobRecommendationsAsync(
                new JobRecommendationsRequest
                {
                    CvText =
                        "C# ASP.NET SQL Docker"
                });

        // Assert
        Assert.Equal(
            2,
            result.Count);

        Assert.Equal(
            highMatchJob.Id,
            result[0].JobId);

        Assert.True(
            result[0].MatchScore >=
            result[1].MatchScore);
    }

    [Fact]
    public async Task JobRecommendationsAsync_ShouldMapJobAndCompanyData()
    {
        // Arrange
        await using var context =
            TestDbContextFactory.Create();

        var company =
            new Company
            {
                Name = "Test Company",
                Location = "Berlin"
            };

        var job =
            new Job
            {
                Title = "Senior Developer",
                Description = "C# und SQL",
                Location = "Berlin",
                Salary = "90.000 EUR",
                Company = company
            };

        context.Companies.Add(company);
        context.Jobs.Add(job);

        await context.SaveChangesAsync();

        var service =
            new AiService(context);

        // Act
        var result =
            await service.JobRecommendationsAsync(
                new JobRecommendationsRequest
                {
                    CvText = "C# SQL"
                });

        // Assert
        var recommendation =
            Assert.Single(result);

        Assert.Equal(
            job.Id,
            recommendation.JobId);

        Assert.Equal(
            job.Title,
            recommendation.Title);

        Assert.Equal(
            company.Name,
            recommendation.Company);

        Assert.Equal(
            job.Location,
            recommendation.Location);

        Assert.Equal(
            job.Salary,
            recommendation.Salary);

        Assert.Equal(
            job.Description,
            recommendation.Description);

        Assert.Equal(
            100,
            recommendation.MatchScore);
    }

    [Fact]
    public async Task JobRecommendationsAsync_ShouldSupportJobWithoutCompany()
    {
        // Arrange
        await using var context =
            TestDbContextFactory.Create();

        var job =
            new Job
            {
                Title = "Developer",
                Description = "C#",
                Location = "Remote",
                Salary = "70.000 EUR",
                CompanyId = null
            };

        context.Jobs.Add(job);
        await context.SaveChangesAsync();

        var service =
            new AiService(context);

        // Act
        var result =
            await service.JobRecommendationsAsync(
                new JobRecommendationsRequest
                {
                    CvText = "C#"
                });

        // Assert
        var recommendation =
            Assert.Single(result);

        Assert.Null(
            recommendation.Company);

        Assert.Equal(
            100,
            recommendation.MatchScore);
    }

    [Fact]
    public void GenerateCoverLetter_ShouldIncludeRequestInformation()
    {
        // Arrange
        using var context =
            TestDbContextFactory.Create();

        var service =
            new AiService(context);

        var request =
            new CoverLetterRequest
            {
                FullName = "Max Mustermann",
                JobTitle = "Backend Developer",
                Company = "SkillJob GmbH",
                CvSummary =
                    "Erfahrung mit C# und ASP.NET Core."
            };

        // Act
        var result =
            service.GenerateCoverLetter(request);

        // Assert
        Assert.Contains(
            request.FullName,
            result.CoverLetter);

        Assert.Contains(
            request.JobTitle,
            result.CoverLetter);

        Assert.Contains(
            request.Company,
            result.CoverLetter);

        Assert.Contains(
            request.CvSummary,
            result.CoverLetter);

        Assert.StartsWith(
            "Sehr geehrte Damen und Herren,",
            result.CoverLetter);

        Assert.EndsWith(
            request.FullName,
            result.CoverLetter);
    }

    [Fact]
    public async Task AnalyzeCvPdfAsync_ShouldReturnNull_WhenFileIsNull()
    {
        // Arrange
        await using var context =
            TestDbContextFactory.Create();

        var service =
            new AiService(context);

        // Act
        var result =
            await service.AnalyzeCvPdfAsync(
                null!);

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public async Task AnalyzeCvPdfAsync_ShouldReturnNull_WhenFileIsEmpty()
    {
        // Arrange
        await using var context =
            TestDbContextFactory.Create();

        var service =
            new AiService(context);

        using var stream =
            new MemoryStream();

        var file =
            new FormFile(
                stream,
                0,
                0,
                "file",
                "empty.pdf");

        // Act
        var result =
            await service.AnalyzeCvPdfAsync(
                file);

        // Assert
        Assert.Null(result);
    }
}