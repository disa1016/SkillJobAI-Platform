using Microsoft.EntityFrameworkCore;
using SkillJobAI.Api.Entities;
using SkillJobAI.Api.Services;
using SkillJobAI.Api.Tests.Helpers;

namespace SkillJobAI.Api.Tests.Services;

public class RecruiterCandidateServiceTests
{
    [Fact]
    public async Task GetCandidatesAsync_ShouldReturnOnlyCandidatesAndStudents()
    {
        // Arrange
        await using var context =
            TestDbContextFactory.Create();

        var candidate =
            CreateUser(
                "Candidate",
                "candidate@test.com");

        var student =
            CreateUser(
                "Student",
                "student@test.com");

        var recruiter =
            CreateUser(
                "Recruiter",
                "recruiter@test.com");

        var admin =
            CreateUser(
                "Admin",
                "admin@test.com");

        context.Users.AddRange(
            candidate,
            student,
            recruiter,
            admin);

        await context.SaveChangesAsync();

        var service =
            new RecruiterCandidateService(
                context);

        // Act
        var result =
            await service.GetCandidatesAsync(
                skill: null);

        // Assert
        Assert.Equal(
            2,
            result.Count);

        Assert.Contains(
            result,
            item =>
                item.Id == candidate.Id);

        Assert.Contains(
            result,
            item =>
                item.Id == student.Id);

        Assert.DoesNotContain(
            result,
            item =>
                item.Id == recruiter.Id);

        Assert.DoesNotContain(
            result,
            item =>
                item.Id == admin.Id);
    }

    [Fact]
    public async Task GetCandidatesAsync_ShouldReturnEmptyList_WhenNoCandidatesExist()
    {
        // Arrange
        await using var context =
            TestDbContextFactory.Create();

        context.Users.AddRange(
            CreateUser(
                "Recruiter",
                "recruiter@test.com"),
            CreateUser(
                "Admin",
                "admin@test.com"));

        await context.SaveChangesAsync();

        var service =
            new RecruiterCandidateService(
                context);

        // Act
        var result =
            await service.GetCandidatesAsync(
                skill: null);

        // Assert
        Assert.Empty(result);
    }

    [Fact]
    public async Task GetCandidatesAsync_ShouldFilterBySkill_CaseInsensitive()
    {
        // Arrange
        await using var context =
            TestDbContextFactory.Create();

        var dotNetCandidate =
            CreateUser(
                "Candidate",
                "dotnet@test.com");

        var vueCandidate =
            CreateUser(
                "Candidate",
                "vue@test.com");

        var dotNetSkill =
            new Skill
            {
                Name = "ASP.NET Core"
            };

        var vueSkill =
            new Skill
            {
                Name = "Vue.js"
            };

        context.Users.AddRange(
            dotNetCandidate,
            vueCandidate);

        context.Skills.AddRange(
            dotNetSkill,
            vueSkill);

        await context.SaveChangesAsync();

        context.UserSkills.AddRange(
            new UserSkill
            {
                UserId =
                    dotNetCandidate.Id,

                SkillId =
                    dotNetSkill.Id
            },
            new UserSkill
            {
                UserId =
                    vueCandidate.Id,

                SkillId =
                    vueSkill.Id
            });

        await context.SaveChangesAsync();

        var service =
            new RecruiterCandidateService(
                context);

        // Act
        var result =
            await service.GetCandidatesAsync(
                "asp.net");

        // Assert
        var candidate =
            Assert.Single(result);

        Assert.Equal(
            dotNetCandidate.Id,
            candidate.Id);

        Assert.Contains(
            "ASP.NET Core",
            candidate.Skills);
    }

    [Fact]
    public async Task GetCandidatesAsync_ShouldSupportPartialSkillSearch()
    {
        // Arrange
        await using var context =
            TestDbContextFactory.Create();

        var candidate =
            CreateUser(
                "Candidate",
                "candidate@test.com");

        var skill =
            new Skill
            {
                Name =
                    "Microsoft Azure"
            };

        context.Users.Add(candidate);
        context.Skills.Add(skill);

        await context.SaveChangesAsync();

        context.UserSkills.Add(
            new UserSkill
            {
                UserId =
                    candidate.Id,

                SkillId =
                    skill.Id
            });

        await context.SaveChangesAsync();

        var service =
            new RecruiterCandidateService(
                context);

        // Act
        var result =
            await service.GetCandidatesAsync(
                "azure");

        // Assert
        Assert.Single(result);
    }

    [Fact]
    public async Task GetCandidatesAsync_ShouldReturnAllCandidates_WhenSkillIsWhitespace()
    {
        // Arrange
        await using var context =
            TestDbContextFactory.Create();

        context.Users.AddRange(
            CreateUser(
                "Candidate",
                "first@test.com"),
            CreateUser(
                "Student",
                "second@test.com"));

        await context.SaveChangesAsync();

        var service =
            new RecruiterCandidateService(
                context);

        // Act
        var result =
            await service.GetCandidatesAsync(
                "   ");

        // Assert
        Assert.Equal(
            2,
            result.Count);
    }

    [Fact]
    public async Task GetCandidatesAsync_ShouldSortBySkillsCountDescending()
    {
        // Arrange
        await using var context =
            TestDbContextFactory.Create();

        var firstCandidate =
            CreateUser(
                "Candidate",
                "first@test.com");

        var secondCandidate =
            CreateUser(
                "Candidate",
                "second@test.com");

        var csharp =
            new Skill
            {
                Name = "C#"
            };

        var sql =
            new Skill
            {
                Name = "SQL"
            };

        context.Users.AddRange(
            firstCandidate,
            secondCandidate);

        context.Skills.AddRange(
            csharp,
            sql);

        await context.SaveChangesAsync();

        context.UserSkills.AddRange(
            new UserSkill
            {
                UserId =
                    firstCandidate.Id,

                SkillId =
                    csharp.Id
            },
            new UserSkill
            {
                UserId =
                    firstCandidate.Id,

                SkillId =
                    sql.Id
            },
            new UserSkill
            {
                UserId =
                    secondCandidate.Id,

                SkillId =
                    csharp.Id
            });

        await context.SaveChangesAsync();

        var service =
            new RecruiterCandidateService(
                context);

        // Act
        var result =
            await service.GetCandidatesAsync(
                null);

        // Assert
        Assert.Equal(
            firstCandidate.Id,
            result[0].Id);

        Assert.Equal(
            2,
            result[0].SkillsCount);

        Assert.Equal(
            secondCandidate.Id,
            result[1].Id);

        Assert.Equal(
            1,
            result[1].SkillsCount);
    }

    [Fact]
    public async Task GetCandidatesAsync_ShouldReturnCorrectApplicationStatistics()
    {
        // Arrange
        await using var context =
            TestDbContextFactory.Create();

        var candidate =
            CreateUser(
                "Candidate",
                "candidate@test.com");

        context.Users.Add(candidate);

        await context.SaveChangesAsync();

        context.Applications.AddRange(
            CreateApplication(
                candidate.Id,
                jobId: 1,
                status: "Accepted"),

            CreateApplication(
                candidate.Id,
                jobId: 2,
                status: "Rejected"),

            CreateApplication(
                candidate.Id,
                jobId: 3,
                status: "Pending"));

        await context.SaveChangesAsync();

        var service =
            new RecruiterCandidateService(
                context);

        // Act
        var result =
            await service.GetCandidatesAsync(
                null);

        // Assert
        var candidateResult =
            Assert.Single(result);

        Assert.Equal(
            3,
            candidateResult.ApplicationsCount);

        Assert.Equal(
            1,
            candidateResult.AcceptedApplications);

        Assert.Equal(
            1,
            candidateResult.RejectedApplications);
    }

    [Fact]
    public async Task GetCandidatesAsync_ShouldReturnZeroCounts_WhenCandidateHasNoSkillsOrApplications()
    {
        // Arrange
        await using var context =
            TestDbContextFactory.Create();

        var candidate =
            CreateUser(
                "Candidate",
                "candidate@test.com");

        context.Users.Add(candidate);
        await context.SaveChangesAsync();

        var service =
            new RecruiterCandidateService(
                context);

        // Act
        var result =
            await service.GetCandidatesAsync(
                null);

        // Assert
        var candidateResult =
            Assert.Single(result);

        Assert.Empty(
            candidateResult.Skills);

        Assert.Equal(
            0,
            candidateResult.SkillsCount);

        Assert.Equal(
            0,
            candidateResult.ApplicationsCount);

        Assert.Equal(
            0,
            candidateResult.AcceptedApplications);

        Assert.Equal(
            0,
            candidateResult.RejectedApplications);
    }

    [Fact]
    public async Task GetCandidateAsync_ShouldReturnNull_WhenUserDoesNotExist()
    {
        // Arrange
        await using var context =
            TestDbContextFactory.Create();

        var service =
            new RecruiterCandidateService(
                context);

        // Act
        var result =
            await service.GetCandidateAsync(
                id: 999);

        // Assert
        Assert.Null(result);
    }

    [Theory]
    [InlineData("Admin")]
    [InlineData("Recruiter")]
    public async Task GetCandidateAsync_ShouldReturnNull_WhenUserIsNotCandidateOrStudent(
        string role)
    {
        // Arrange
        await using var context =
            TestDbContextFactory.Create();

        var user =
            CreateUser(
                role,
                $"{role.ToLower()}@test.com");

        context.Users.Add(user);
        await context.SaveChangesAsync();

        var service =
            new RecruiterCandidateService(
                context);

        // Act
        var result =
            await service.GetCandidateAsync(
                user.Id);

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public async Task GetCandidateAsync_ShouldReturnCandidateDetails_WhenCandidateExists()
    {
        // Arrange
        await using var context =
            TestDbContextFactory.Create();

        var createdAt =
            DateTime.UtcNow.AddDays(-5);

        var candidate =
            new AppUser
            {
                FullName =
                    "Detailed Candidate",

                Email =
                    "details@test.com",

                PasswordHash =
                    "hash",

                Role =
                    "Candidate",

                CvUrl =
                    "/uploads/cv/details.pdf",

                CreatedAt =
                    createdAt
            };

        context.Users.Add(candidate);
        await context.SaveChangesAsync();

        var service =
            new RecruiterCandidateService(
                context);

        // Act
        var result =
            await service.GetCandidateAsync(
                candidate.Id);

        // Assert
        Assert.NotNull(result);

        Assert.Equal(
            candidate.Id,
            result.Id);

        Assert.Equal(
            candidate.FullName,
            result.FullName);

        Assert.Equal(
            candidate.Email,
            result.Email);

        Assert.Equal(
            candidate.CvUrl,
            result.CvUrl);

        Assert.Equal(
            createdAt,
            result.CreatedAt);
    }

    [Fact]
    public async Task GetCandidateAsync_ShouldReturnSkillsAndSkillCount()
    {
        // Arrange
        await using var context =
            TestDbContextFactory.Create();

        var candidate =
            CreateUser(
                "Student",
                "student@test.com");

        var csharp =
            new Skill
            {
                Name = "C#"
            };

        var docker =
            new Skill
            {
                Name = "Docker"
            };

        context.Users.Add(candidate);
        context.Skills.AddRange(
            csharp,
            docker);

        await context.SaveChangesAsync();

        context.UserSkills.AddRange(
            new UserSkill
            {
                UserId =
                    candidate.Id,

                SkillId =
                    csharp.Id
            },
            new UserSkill
            {
                UserId =
                    candidate.Id,

                SkillId =
                    docker.Id
            });

        await context.SaveChangesAsync();

        var service =
            new RecruiterCandidateService(
                context);

        // Act
        var result =
            await service.GetCandidateAsync(
                candidate.Id);

        // Assert
        Assert.NotNull(result);

        Assert.Equal(
            2,
            result.SkillsCount);

        Assert.Contains(
            "C#",
            result.Skills);

        Assert.Contains(
            "Docker",
            result.Skills);
    }

    [Fact]
    public async Task GetCandidateAsync_ShouldReturnApplicationsOrderedByCreatedAtDescending()
    {
        // Arrange
        await using var context =
            TestDbContextFactory.Create();

        var candidate =
            CreateUser(
                "Candidate",
                "candidate@test.com");

        context.Users.Add(candidate);
        await context.SaveChangesAsync();

        var olderApplication =
            CreateApplication(
                candidate.Id,
                jobId: 1,
                status: "Pending",
                createdAt:
                    DateTime.UtcNow
                        .AddDays(-2));

        var newerApplication =
            CreateApplication(
                candidate.Id,
                jobId: 2,
                status: "Accepted",
                createdAt:
                    DateTime.UtcNow
                        .AddDays(-1));

        context.Applications.AddRange(
            olderApplication,
            newerApplication);

        await context.SaveChangesAsync();

        var service =
            new RecruiterCandidateService(
                context);

        // Act
        var result =
            await service.GetCandidateAsync(
                candidate.Id);

        // Assert
        Assert.NotNull(result);

        Assert.Equal(
            2,
            result.ApplicationsCount);

        Assert.Equal(
            newerApplication.Id,
            result.Applications[0].Id);

        Assert.Equal(
            olderApplication.Id,
            result.Applications[1].Id);
    }

    [Fact]
    public async Task GetCandidateAsync_ShouldReturnApplicationStatistics()
    {
        // Arrange
        await using var context =
            TestDbContextFactory.Create();

        var candidate =
            CreateUser(
                "Candidate",
                "candidate@test.com");

        context.Users.Add(candidate);
        await context.SaveChangesAsync();

        context.Applications.AddRange(
            CreateApplication(
                candidate.Id,
                1,
                "Accepted"),

            CreateApplication(
                candidate.Id,
                2,
                "Accepted"),

            CreateApplication(
                candidate.Id,
                3,
                "Rejected"),

            CreateApplication(
                candidate.Id,
                4,
                "Pending"));

        await context.SaveChangesAsync();

        var service =
            new RecruiterCandidateService(
                context);

        // Act
        var result =
            await service.GetCandidateAsync(
                candidate.Id);

        // Assert
        Assert.NotNull(result);

        Assert.Equal(
            4,
            result.ApplicationsCount);

        Assert.Equal(
            2,
            result.AcceptedApplications);

        Assert.Equal(
            1,
            result.RejectedApplications);
    }

    [Fact]
    public async Task GetCandidateAsync_ShouldReturnJobAndCompanyInformation()
    {
        // Arrange
        await using var context =
            TestDbContextFactory.Create();

        var candidate =
            CreateUser(
                "Candidate",
                "candidate@test.com");

        var company =
            new Company
            {
                Name =
                    "SkillJob Test Company",

                Location =
                    "Berlin",

                CreatedAt =
                    DateTime.UtcNow
            };

        var job =
            new Job
            {
                Title =
                    "Backend Developer",

                Description =
                    "ASP.NET Core position",

                Location =
                    "Berlin",

                Salary =
                    "80.000 EUR",

                Company =
                    company,

                CreatedAt =
                    DateTime.UtcNow
            };

        context.Users.Add(candidate);
        context.Companies.Add(company);
        context.Jobs.Add(job);

        await context.SaveChangesAsync();

        var application =
            new Application
            {
                UserId =
                    candidate.Id,

                JobId =
                    job.Id,

                CoverLetter =
                    "Test cover letter",

                Status =
                    "Pending",

                CvFileUrl =
                    "/private_uploads/cv.pdf",

                CertificateFileUrl =
                    "/private_uploads/certificate.pdf",

                PortfolioFileUrl =
                    "/private_uploads/portfolio.pdf",

                CreatedAt =
                    DateTime.UtcNow
            };

        context.Applications.Add(
            application);

        await context.SaveChangesAsync();

        var service =
            new RecruiterCandidateService(
                context);

        // Act
        var result =
            await service.GetCandidateAsync(
                candidate.Id);

        // Assert
        Assert.NotNull(result);

        var applicationResult =
            Assert.Single(
                result.Applications);

        Assert.Equal(
            application.Id,
            applicationResult.Id);

        Assert.Equal(
            application.JobId,
            applicationResult.JobId);

        Assert.Equal(
            application.CoverLetter,
            applicationResult.CoverLetter);

        Assert.Equal(
            application.Status,
            applicationResult.Status);

        Assert.Equal(
            application.CvFileUrl,
            applicationResult.CvFileUrl);

        Assert.Equal(
            application.CertificateFileUrl,
            applicationResult.CertificateFileUrl);

        Assert.Equal(
            application.PortfolioFileUrl,
            applicationResult.PortfolioFileUrl);

        Assert.NotNull(
            applicationResult.Job);

        Assert.Equal(
            job.Id,
            applicationResult.Job.Id);

        Assert.Equal(
            job.Title,
            applicationResult.Job.Title);

        Assert.Equal(
            company.Name,
            applicationResult.Job.Company);

        Assert.Equal(
            job.Location,
            applicationResult.Job.Location);

        Assert.Equal(
            job.Salary,
            applicationResult.Job.Salary);
    }

    [Fact]
    public async Task GetCandidateAsync_ShouldReturnNullJob_WhenApplicationJobDoesNotExist()
    {
        // Arrange
        await using var context =
            TestDbContextFactory.Create();

        var candidate =
            CreateUser(
                "Candidate",
                "candidate@test.com");

        context.Users.Add(candidate);
        await context.SaveChangesAsync();

        context.Applications.Add(
            CreateApplication(
                candidate.Id,
                jobId: 999,
                status: "Pending"));

        await context.SaveChangesAsync();

        var service =
            new RecruiterCandidateService(
                context);

        // Act
        var result =
            await service.GetCandidateAsync(
                candidate.Id);

        // Assert
        Assert.NotNull(result);

        var application =
            Assert.Single(
                result.Applications);

        Assert.Null(
            application.Job);
    }

    private static AppUser CreateUser(
        string role,
        string email)
    {
        return new AppUser
        {
            FullName =
                $"{role} Test User",

            Email =
                email,

            PasswordHash =
                "test-password-hash",

            Role =
                role,

            CreatedAt =
                DateTime.UtcNow
        };
    }

    private static Application CreateApplication(
        int userId,
        int jobId,
        string status,
        DateTime? createdAt = null)
    {
        return new Application
        {
            UserId =
                userId,

            JobId =
                jobId,

            CoverLetter =
                "Test cover letter",

            Status =
                status,

            CreatedAt =
                createdAt ??
                DateTime.UtcNow
        };
    }
}