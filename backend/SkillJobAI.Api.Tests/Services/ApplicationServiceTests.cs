using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Moq;
using SkillJobAI.Api.Entities;
using SkillJobAI.Api.Models;
using SkillJobAI.Api.Services;
using SkillJobAI.Api.Tests.Helpers;

namespace SkillJobAI.Api.Tests.Services;

public class ApplicationServiceTests
{
    [Fact]
    public async Task CreateApplicationAsync_ShouldReturnNull_WhenJobDoesNotExist()
    {
        // Arrange
        await using var context =
            TestDbContextFactory.Create();

        var fileStorageServiceMock =
            new Mock<IFileStorageService>(
                MockBehavior.Strict);

        var applicationMatchingServiceMock =
            new Mock<IApplicationMatchingService>(
                MockBehavior.Strict);

        var emailServiceMock =
            new Mock<IEmailService>(
                MockBehavior.Strict);

        var applicationService =
            CreateApplicationService(
                context,
                fileStorageServiceMock,
                applicationMatchingServiceMock,
                emailServiceMock);

        var request =
            new CreateApplicationRequest
            {
                JobId = 999,
                CoverLetter = "Test application"
            };

        // Act
        var result =
            await applicationService.CreateApplicationAsync(
                userId: 1,
                request);

        // Assert
        Assert.Null(result);

        Assert.Empty(
            await context.Applications
                .ToListAsync());

        fileStorageServiceMock.Verify(
            service =>
                service.SavePdfFileAsync(
                    It.IsAny<IFormFile?>(),
                    It.IsAny<string>(),
                    It.IsAny<int>()),
            Times.Never);

        fileStorageServiceMock.VerifyNoOtherCalls();
        applicationMatchingServiceMock.VerifyNoOtherCalls();
        emailServiceMock.VerifyNoOtherCalls();
    }

    [Fact]
    public async Task CreateApplicationAsync_ShouldCreateApplication_WhenJobExists()
    {
        // Arrange
        await using var context =
            TestDbContextFactory.Create();

        var company = CreateCompany();
        var candidate = CreateCandidate();
        var job = CreateJob(company);

        context.Companies.Add(company);
        context.Users.Add(candidate);
        context.Jobs.Add(job);

        await context.SaveChangesAsync();

        var cvFile = CreateFormFile("cv.pdf");
        var certificateFile =
            CreateFormFile("certificate.pdf");
        var portfolioFile =
            CreateFormFile("portfolio.pdf");

        var fileStorageServiceMock =
            new Mock<IFileStorageService>(
                MockBehavior.Strict);

        fileStorageServiceMock
            .Setup(service =>
                service.SavePdfFileAsync(
                    cvFile,
                    "cv",
                    candidate.Id))
            .ReturnsAsync(
                "/private_uploads/cv/test-cv.pdf");

        fileStorageServiceMock
            .Setup(service =>
                service.SavePdfFileAsync(
                    certificateFile,
                    "certificates",
                    candidate.Id))
            .ReturnsAsync(
                "/private_uploads/certificates/test-certificate.pdf");

        fileStorageServiceMock
            .Setup(service =>
                service.SavePdfFileAsync(
                    portfolioFile,
                    "portfolio",
                    candidate.Id))
            .ReturnsAsync(
                "/private_uploads/portfolio/test-portfolio.pdf");

        var matchingServiceMock =
            CreateMatchingServiceMock(
                job.Id,
                candidate.Id);

        var emailServiceMock =
            new Mock<IEmailService>(
                MockBehavior.Strict);

        var applicationService =
            CreateApplicationService(
                context,
                fileStorageServiceMock,
                matchingServiceMock,
                emailServiceMock);

        var request =
            new CreateApplicationRequest
            {
                JobId = job.Id,
                CoverLetter =
                    "I would like to apply for this job.",
                CvFile = cvFile,
                CertificateFile = certificateFile,
                PortfolioFile = portfolioFile
            };

        var beforeCreation =
            DateTime.UtcNow;

        // Act
        var result =
            await applicationService.CreateApplicationAsync(
                candidate.Id,
                request);

        var afterCreation =
            DateTime.UtcNow;

        // Assert
        Assert.NotNull(result);

        var storedApplication =
            await context.Applications
                .SingleAsync();

        Assert.Equal(
            candidate.Id,
            storedApplication.UserId);

        Assert.Equal(
            job.Id,
            storedApplication.JobId);

        Assert.Equal(
            request.CoverLetter,
            storedApplication.CoverLetter);

        Assert.Equal(
            "Pending",
            storedApplication.Status);

        Assert.Equal(
            "/private_uploads/cv/test-cv.pdf",
            storedApplication.CvFileUrl);

        Assert.Equal(
            "/private_uploads/certificates/test-certificate.pdf",
            storedApplication.CertificateFileUrl);

        Assert.Equal(
            "/private_uploads/portfolio/test-portfolio.pdf",
            storedApplication.PortfolioFileUrl);

        Assert.InRange(
            storedApplication.CreatedAt,
            beforeCreation,
            afterCreation);

        Assert.Equal(
            storedApplication.Id,
            result.Id);

        Assert.Equal(
            candidate.Id,
            result.UserId);

        Assert.Equal(
            job.Id,
            result.JobId);

        Assert.Equal(
            "Pending",
            result.Status);

        Assert.NotNull(result.Candidate);
        Assert.Equal(
            candidate.Email,
            result.Candidate.Email);

        Assert.NotNull(result.Job);
        Assert.Equal(
            job.Title,
            result.Job.Title);

        Assert.Equal(
            company.Name,
            result.Job.Company);

        Assert.Equal(
            80,
            result.MatchPercentage);

        Assert.Contains(
            "C#",
            result.MatchedSkills);

        fileStorageServiceMock.VerifyAll();
        matchingServiceMock.VerifyAll();
        emailServiceMock.VerifyNoOtherCalls();
    }

    [Fact]
    public async Task CreateApplicationAsync_ShouldThrow_WhenActiveApplicationAlreadyExists()
    {
        // Arrange
        await using var context =
            TestDbContextFactory.Create();

        var company = CreateCompany();
        var candidate = CreateCandidate();
        var job = CreateJob(company);

        context.Companies.Add(company);
        context.Users.Add(candidate);
        context.Jobs.Add(job);

        await context.SaveChangesAsync();

        context.Applications.Add(
            new Application
            {
                UserId = candidate.Id,
                JobId = job.Id,
                CoverLetter = "Existing application",
                Status = "Pending",
                CreatedAt = DateTime.UtcNow
            });

        await context.SaveChangesAsync();

        var fileStorageServiceMock =
            new Mock<IFileStorageService>(
                MockBehavior.Strict);

        var matchingServiceMock =
            new Mock<IApplicationMatchingService>(
                MockBehavior.Strict);

        var emailServiceMock =
            new Mock<IEmailService>(
                MockBehavior.Strict);

        var applicationService =
            CreateApplicationService(
                context,
                fileStorageServiceMock,
                matchingServiceMock,
                emailServiceMock);

        var request =
            new CreateApplicationRequest
            {
                JobId = job.Id,
                CoverLetter = "Duplicate application"
            };

        // Act
        var exception =
            await Assert.ThrowsAsync<InvalidOperationException>(
                () =>
                    applicationService.CreateApplicationAsync(
                        candidate.Id,
                        request));

        // Assert
        Assert.Equal(
            "Du hast bereits eine aktive Bewerbung für diesen Job.",
            exception.Message);

        Assert.Equal(
            1,
            await context.Applications.CountAsync());

        fileStorageServiceMock.VerifyNoOtherCalls();
        matchingServiceMock.VerifyNoOtherCalls();
        emailServiceMock.VerifyNoOtherCalls();
    }

    [Fact]
    public async Task CreateApplicationAsync_ShouldAllowNewApplication_WhenPreviousApplicationWasRejected()
    {
        // Arrange
        await using var context =
            TestDbContextFactory.Create();

        var company = CreateCompany();
        var candidate = CreateCandidate();
        var job = CreateJob(company);

        context.Companies.Add(company);
        context.Users.Add(candidate);
        context.Jobs.Add(job);

        await context.SaveChangesAsync();

        context.Applications.Add(
            new Application
            {
                UserId = candidate.Id,
                JobId = job.Id,
                CoverLetter = "Rejected application",
                Status = "Rejected",
                CreatedAt = DateTime.UtcNow.AddDays(-5)
            });

        await context.SaveChangesAsync();

        var fileStorageServiceMock =
            new Mock<IFileStorageService>(
                MockBehavior.Strict);

        fileStorageServiceMock
            .Setup(service =>
                service.SavePdfFileAsync(
                    null,
                    "cv",
                    candidate.Id))
            .ReturnsAsync((string?)null);

        fileStorageServiceMock
            .Setup(service =>
                service.SavePdfFileAsync(
                    null,
                    "certificates",
                    candidate.Id))
            .ReturnsAsync((string?)null);

        fileStorageServiceMock
            .Setup(service =>
                service.SavePdfFileAsync(
                    null,
                    "portfolio",
                    candidate.Id))
            .ReturnsAsync((string?)null);

        var matchingServiceMock =
            CreateMatchingServiceMock(
                job.Id,
                candidate.Id);

        var emailServiceMock =
            new Mock<IEmailService>(
                MockBehavior.Strict);

        var applicationService =
            CreateApplicationService(
                context,
                fileStorageServiceMock,
                matchingServiceMock,
                emailServiceMock);

        var request =
            new CreateApplicationRequest
            {
                JobId = job.Id,
                CoverLetter = "New application"
            };

        // Act
        var result =
            await applicationService.CreateApplicationAsync(
                candidate.Id,
                request);

        // Assert
        Assert.NotNull(result);

        var applications =
            await context.Applications
                .OrderBy(application =>
                    application.CreatedAt)
                .ToListAsync();

        Assert.Equal(
            2,
            applications.Count);

        Assert.Equal(
            "Rejected",
            applications[0].Status);

        Assert.Equal(
            "Pending",
            applications[1].Status);

        fileStorageServiceMock.VerifyAll();
        matchingServiceMock.VerifyAll();
        emailServiceMock.VerifyNoOtherCalls();
    }

    [Fact]
    public async Task GetMyApplicationsAsync_ShouldReturnOnlyApplicationsForCurrentUser()
    {
        // Arrange
        await using var context =
            TestDbContextFactory.Create();

        var company = CreateCompany();
        var firstCandidate =
            CreateCandidate(
                "First Candidate",
                "first@test.com");

        var secondCandidate =
            CreateCandidate(
                "Second Candidate",
                "second@test.com");

        var firstJob =
            CreateJob(
                company,
                "Backend Developer");

        var secondJob =
            CreateJob(
                company,
                "Frontend Developer");

        context.Companies.Add(company);

        context.Users.AddRange(
            firstCandidate,
            secondCandidate);

        context.Jobs.AddRange(
            firstJob,
            secondJob);

        await context.SaveChangesAsync();

        context.Applications.AddRange(
            new Application
            {
                UserId = firstCandidate.Id,
                JobId = firstJob.Id,
                CoverLetter = "First application",
                Status = "Pending",
                CreatedAt = DateTime.UtcNow
            },
            new Application
            {
                UserId = firstCandidate.Id,
                JobId = secondJob.Id,
                CoverLetter = "Second application",
                Status = "Accepted",
                CreatedAt = DateTime.UtcNow
            },
            new Application
            {
                UserId = secondCandidate.Id,
                JobId = firstJob.Id,
                CoverLetter = "Other candidate",
                Status = "Pending",
                CreatedAt = DateTime.UtcNow
            });

        await context.SaveChangesAsync();

        var applicationService =
            CreateApplicationService(
                context);

        // Act
        var result =
            await applicationService
                .GetMyApplicationsAsync(
                    firstCandidate.Id);

        // Assert
        Assert.Equal(
            2,
            result.Count);

        Assert.All(
            result,
            application =>
                Assert.Equal(
                    firstCandidate.Id,
                    application.UserId));

        Assert.Contains(
            result,
            application =>
                application.Job?.Title ==
                "Backend Developer");

        Assert.Contains(
            result,
            application =>
                application.Job?.Title ==
                "Frontend Developer");

        Assert.All(
            result,
            application =>
                Assert.Equal(
                    company.Name,
                    application.Job?.Company));
    }

    [Fact]
    public async Task GetApplicationByIdAsync_ShouldReturnNull_WhenApplicationDoesNotExist()
    {
        // Arrange
        await using var context =
            TestDbContextFactory.Create();

        var applicationService =
            CreateApplicationService(
                context);

        // Act
        var result =
            await applicationService
                .GetApplicationByIdAsync(
                    id: 999);

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public async Task GetApplicationByIdAsync_ShouldReturnApplicationWithCandidateJobAndMatch()
    {
        // Arrange
        await using var context =
            TestDbContextFactory.Create();

        var company = CreateCompany();
        var candidate = CreateCandidate();
        var job = CreateJob(company);

        context.Companies.Add(company);
        context.Users.Add(candidate);
        context.Jobs.Add(job);

        await context.SaveChangesAsync();

        var application =
            new Application
            {
                UserId = candidate.Id,
                JobId = job.Id,
                CoverLetter = "Detailed application",
                Status = "Pending",
                CvFileUrl =
                    "/private_uploads/cv/test.pdf",
                CreatedAt = DateTime.UtcNow
            };

        context.Applications.Add(application);

        await context.SaveChangesAsync();

        var matchingServiceMock =
            CreateMatchingServiceMock(
                job.Id,
                candidate.Id);

        var applicationService =
            CreateApplicationService(
                context,
                applicationMatchingServiceMock:
                    matchingServiceMock);

        // Act
        var result =
            await applicationService
                .GetApplicationByIdAsync(
                    application.Id);

        // Assert
        Assert.NotNull(result);

        Assert.Equal(
            application.Id,
            result.Id);

        Assert.Equal(
            candidate.Id,
            result.UserId);

        Assert.Equal(
            job.Id,
            result.JobId);

        Assert.Equal(
            application.CoverLetter,
            result.CoverLetter);

        Assert.NotNull(result.Candidate);

        Assert.Equal(
            candidate.FullName,
            result.Candidate.FullName);

        Assert.Equal(
            candidate.Email,
            result.Candidate.Email);

        Assert.NotNull(result.Job);

        Assert.Equal(
            job.Title,
            result.Job.Title);

        Assert.Equal(
            company.Name,
            result.Job.Company);

        Assert.Equal(
            80,
            result.MatchPercentage);

        Assert.Equal(
            new[] { "C#", "ASP.NET Core" },
            result.JobSkills);

        Assert.Equal(
            new[] { "C#", "SQL" },
            result.UserSkills);

        matchingServiceMock.VerifyAll();
    }

    [Fact]
    public async Task UpdateApplicationStatusAsync_ShouldReturnNull_WhenApplicationDoesNotExist()
    {
        // Arrange
        await using var context =
            TestDbContextFactory.Create();

        var emailServiceMock =
            new Mock<IEmailService>(
                MockBehavior.Strict);

        var applicationService =
            CreateApplicationService(
                context,
                emailServiceMock:
                    emailServiceMock);

        // Act
        var result =
            await applicationService
                .UpdateApplicationStatusAsync(
                    id: 999,
                    status: "Accepted");

        // Assert
        Assert.Null(result);

        emailServiceMock.VerifyNoOtherCalls();
    }

    [Fact]
    public async Task UpdateApplicationStatusAsync_ShouldUpdateStatusAndSendEmail()
    {
        // Arrange
        await using var context =
            TestDbContextFactory.Create();

        var company = CreateCompany();
        var candidate = CreateCandidate();
        var job = CreateJob(company);

        context.Companies.Add(company);
        context.Users.Add(candidate);
        context.Jobs.Add(job);

        await context.SaveChangesAsync();

        var application =
            new Application
            {
                UserId = candidate.Id,
                JobId = job.Id,
                CoverLetter = "Application",
                Status = "Pending",
                CreatedAt = DateTime.UtcNow
            };

        context.Applications.Add(application);

        await context.SaveChangesAsync();

        var matchingServiceMock =
            CreateMatchingServiceMock(
                job.Id,
                candidate.Id);

        var emailServiceMock =
            new Mock<IEmailService>(
                MockBehavior.Strict);

        emailServiceMock
            .Setup(service =>
                service.SendEmailAsync(
                    candidate.Email,
                    "Dein Bewerbungsstatus wurde aktualisiert - SkillJob AI",
                    It.Is<string>(body =>
                        body.Contains(candidate.FullName) &&
                        body.Contains(job.Title) &&
                        body.Contains(company.Name) &&
                        body.Contains("Accepted"))))
            .Returns(Task.CompletedTask);

        var applicationService =
            CreateApplicationService(
                context,
                applicationMatchingServiceMock:
                    matchingServiceMock,
                emailServiceMock:
                    emailServiceMock);

        // Act
        var result =
            await applicationService
                .UpdateApplicationStatusAsync(
                    application.Id,
                    "Accepted");

        // Assert
        Assert.NotNull(result);

        Assert.Equal(
            "Accepted",
            result.Status);

        var storedApplication =
            await context.Applications
                .SingleAsync();

        Assert.Equal(
            "Accepted",
            storedApplication.Status);

        emailServiceMock.VerifyAll();
        matchingServiceMock.VerifyAll();
    }

    [Fact]
    public async Task GetApplicationsForJobAsync_ShouldReturnPagedApplications()
    {
        // Arrange
        await using var context =
            TestDbContextFactory.Create();

        var company = CreateCompany();
        var job = CreateJob(company);

        var firstCandidate =
            CreateCandidate(
                "Candidate One",
                "one@test.com");

        var secondCandidate =
            CreateCandidate(
                "Candidate Two",
                "two@test.com");

        var thirdCandidate =
            CreateCandidate(
                "Candidate Three",
                "three@test.com");

        context.Companies.Add(company);
        context.Jobs.Add(job);

        context.Users.AddRange(
            firstCandidate,
            secondCandidate,
            thirdCandidate);

        await context.SaveChangesAsync();

        context.Applications.AddRange(
            CreateApplication(
                firstCandidate.Id,
                job.Id,
                "First",
                DateTime.UtcNow.AddMinutes(-3)),
            CreateApplication(
                secondCandidate.Id,
                job.Id,
                "Second",
                DateTime.UtcNow.AddMinutes(-2)),
            CreateApplication(
                thirdCandidate.Id,
                job.Id,
                "Third",
                DateTime.UtcNow.AddMinutes(-1)));

        await context.SaveChangesAsync();

        var matchingServiceMock =
            new Mock<IApplicationMatchingService>(
                MockBehavior.Strict);

        matchingServiceMock
            .Setup(service =>
                service.GetMatchResultAsync(
                    job.Id,
                    thirdCandidate.Id))
            .ReturnsAsync(
                CreateMatchResult());

        matchingServiceMock
            .Setup(service =>
                service.GetMatchResultAsync(
                    job.Id,
                    secondCandidate.Id))
            .ReturnsAsync(
                CreateMatchResult());

        var applicationService =
            CreateApplicationService(
                context,
                applicationMatchingServiceMock:
                    matchingServiceMock);

        // Act
        var result =
            await applicationService
                .GetApplicationsForJobAsync(
                    job.Id,
                    page: 1,
                    pageSize: 2,
                    search: null,
                    status: null);

        // Assert
        Assert.Equal(
            3,
            result.TotalItems);

        Assert.Equal(
            2,
            result.TotalPages);

        Assert.Equal(
            1,
            result.Page);

        Assert.Equal(
            2,
            result.PageSize);

        Assert.Equal(
            2,
            result.Items.Count);

        Assert.Equal(
            thirdCandidate.Id,
            result.Items[0].UserId);

        Assert.Equal(
            secondCandidate.Id,
            result.Items[1].UserId);

        matchingServiceMock.VerifyAll();
    }

    [Fact]
    public async Task GetApplicationsForJobAsync_ShouldFilterByStatus()
    {
        // Arrange
        await using var context =
            TestDbContextFactory.Create();

        var company = CreateCompany();
        var job = CreateJob(company);

        var pendingCandidate =
            CreateCandidate(
                "Pending Candidate",
                "pending@test.com");

        var acceptedCandidate =
            CreateCandidate(
                "Accepted Candidate",
                "accepted@test.com");

        context.Companies.Add(company);
        context.Jobs.Add(job);

        context.Users.AddRange(
            pendingCandidate,
            acceptedCandidate);

        await context.SaveChangesAsync();

        context.Applications.AddRange(
            new Application
            {
                UserId = pendingCandidate.Id,
                JobId = job.Id,
                CoverLetter = "Pending application",
                Status = "Pending",
                CreatedAt = DateTime.UtcNow
            },
            new Application
            {
                UserId = acceptedCandidate.Id,
                JobId = job.Id,
                CoverLetter = "Accepted application",
                Status = "Accepted",
                CreatedAt = DateTime.UtcNow
            });

        await context.SaveChangesAsync();

        var matchingServiceMock =
            CreateMatchingServiceMock(
                job.Id,
                acceptedCandidate.Id);

        var applicationService =
            CreateApplicationService(
                context,
                applicationMatchingServiceMock:
                    matchingServiceMock);

        // Act
        var result =
            await applicationService
                .GetApplicationsForJobAsync(
                    job.Id,
                    page: 1,
                    pageSize: 10,
                    search: null,
                    status: "Accepted");

        // Assert
        Assert.Single(result.Items);

        Assert.Equal(
            acceptedCandidate.Id,
            result.Items[0].UserId);

        Assert.Equal(
            "Accepted",
            result.Items[0].Status);

        Assert.Equal(
            1,
            result.TotalItems);

        matchingServiceMock.VerifyAll();
    }

    [Fact]
    public async Task GetApplicationsForJobAsync_ShouldFilterBySearch()
    {
        // Arrange
        await using var context =
            TestDbContextFactory.Create();

        var company = CreateCompany();
        var job = CreateJob(company);

        var matchingCandidate =
            CreateCandidate(
                "Maria Developer",
                "maria@test.com");

        var otherCandidate =
            CreateCandidate(
                "John Tester",
                "john@test.com");

        context.Companies.Add(company);
        context.Jobs.Add(job);

        context.Users.AddRange(
            matchingCandidate,
            otherCandidate);

        await context.SaveChangesAsync();

        context.Applications.AddRange(
            CreateApplication(
                matchingCandidate.Id,
                job.Id,
                "Experienced backend developer",
                DateTime.UtcNow),
            CreateApplication(
                otherCandidate.Id,
                job.Id,
                "Quality assurance specialist",
                DateTime.UtcNow));

        await context.SaveChangesAsync();

        var matchingServiceMock =
            CreateMatchingServiceMock(
                job.Id,
                matchingCandidate.Id);

        var applicationService =
            CreateApplicationService(
                context,
                applicationMatchingServiceMock:
                    matchingServiceMock);

        // Act
        var result =
            await applicationService
                .GetApplicationsForJobAsync(
                    job.Id,
                    page: 1,
                    pageSize: 10,
                    search: "maria",
                    status: null);

        // Assert
        Assert.Single(result.Items);

        Assert.Equal(
            matchingCandidate.Id,
            result.Items[0].UserId);

        Assert.Equal(
            "Maria Developer",
            result.Items[0].Candidate?.FullName);

        matchingServiceMock.VerifyAll();
    }

    [Fact]
    public async Task GetApplicationsForJobAsync_ShouldNormalizeInvalidPagination()
    {
        // Arrange
        await using var context =
            TestDbContextFactory.Create();

        var applicationService =
            CreateApplicationService(
                context);

        // Act
        var result =
            await applicationService
                .GetApplicationsForJobAsync(
                    jobId: 1,
                    page: 0,
                    pageSize: 0,
                    search: null,
                    status: null);

        // Assert
        Assert.Equal(
            1,
            result.Page);

        Assert.Equal(
            10,
            result.PageSize);

        Assert.Empty(result.Items);
    }

    [Fact]
    public async Task GetApplicationsForJobAsync_ShouldLimitPageSizeToFifty()
    {
        // Arrange
        await using var context =
            TestDbContextFactory.Create();

        var applicationService =
            CreateApplicationService(
                context);

        // Act
        var result =
            await applicationService
                .GetApplicationsForJobAsync(
                    jobId: 1,
                    page: 1,
                    pageSize: 100,
                    search: null,
                    status: null);

        // Assert
        Assert.Equal(
            50,
            result.PageSize);
    }

    [Fact]
    public async Task GetApplicationCompanyIdAsync_ShouldReturnCompanyId()
    {
        // Arrange
        await using var context =
            TestDbContextFactory.Create();

        var company = CreateCompany();
        var candidate = CreateCandidate();
        var job = CreateJob(company);

        context.Companies.Add(company);
        context.Users.Add(candidate);
        context.Jobs.Add(job);

        await context.SaveChangesAsync();

        var application =
            CreateApplication(
                candidate.Id,
                job.Id,
                "Application",
                DateTime.UtcNow);

        context.Applications.Add(application);

        await context.SaveChangesAsync();

        var applicationService =
            CreateApplicationService(
                context);

        // Act
        var companyId =
            await applicationService
                .GetApplicationCompanyIdAsync(
                    application.Id);

        // Assert
        Assert.Equal(
            company.Id,
            companyId);
    }

    [Fact]
    public async Task GetJobCompanyIdAsync_ShouldReturnCompanyId()
    {
        // Arrange
        await using var context =
            TestDbContextFactory.Create();

        var company = CreateCompany();
        var job = CreateJob(company);

        context.Companies.Add(company);
        context.Jobs.Add(job);

        await context.SaveChangesAsync();

        var applicationService =
            CreateApplicationService(
                context);

        // Act
        var companyId =
            await applicationService
                .GetJobCompanyIdAsync(
                    job.Id);

        // Assert
        Assert.Equal(
            company.Id,
            companyId);
    }

    [Fact]
    public async Task GetApplicationFileAsync_ShouldReturnNull_WhenApplicationDoesNotExist()
    {
        // Arrange
        await using var context =
            TestDbContextFactory.Create();

        var applicationService =
            CreateApplicationService(
                context);

        // Act
        var result =
            await applicationService
                .GetApplicationFileAsync(
                    applicationId: 999,
                    fileType: "cv");

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public async Task GetApplicationFileAsync_ShouldReturnNull_WhenFileTypeIsInvalid()
    {
        // Arrange
        await using var context =
            TestDbContextFactory.Create();

        var application =
            new Application
            {
                UserId = 1,
                JobId = 1,
                CoverLetter = "Application",
                Status = "Pending",
                CvFileUrl =
                    "/private_uploads/cv/test.pdf",
                CreatedAt = DateTime.UtcNow
            };

        context.Applications.Add(application);

        await context.SaveChangesAsync();

        var applicationService =
            CreateApplicationService(
                context);

        // Act
        var result =
            await applicationService
                .GetApplicationFileAsync(
                    application.Id,
                    "unknown");

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public async Task GetApplicationFileAsync_ShouldReturnNull_WhenFileDoesNotExist()
    {
        // Arrange
        await using var context =
            TestDbContextFactory.Create();

        var application =
            new Application
            {
                UserId = 1,
                JobId = 1,
                CoverLetter = "Application",
                Status = "Pending",
                CvFileUrl =
                    "/private_uploads/cv/missing-file.pdf",
                CreatedAt = DateTime.UtcNow
            };

        context.Applications.Add(application);

        await context.SaveChangesAsync();

        var applicationService =
            CreateApplicationService(
                context);

        // Act
        var result =
            await applicationService
                .GetApplicationFileAsync(
                    application.Id,
                    "cv");

        // Assert
        Assert.Null(result);
    }

    [Theory]
    [InlineData(
        "cv",
        "cv",
        "cv-application-")]
    [InlineData(
        "certificate",
        "certificates",
        "certificate-application-")]
    [InlineData(
        "portfolio",
        "portfolio",
        "portfolio-application-")]
    public async Task GetApplicationFileAsync_ShouldReturnFile_WhenPrivateUploadExists(
        string fileType,
        string folderName,
        string expectedDownloadPrefix)
    {
        // Arrange
        await using var context =
            TestDbContextFactory.Create();

        var uniqueFileName =
            $"{Guid.NewGuid():N}.pdf";

        var relativePath =
            Path.Combine(
                "private_uploads",
                folderName,
                uniqueFileName);

        var physicalPath =
            Path.Combine(
                Directory.GetCurrentDirectory(),
                relativePath);

        var directory =
            Path.GetDirectoryName(
                physicalPath)!;

        Directory.CreateDirectory(
            directory);

        await File.WriteAllBytesAsync(
            physicalPath,
            new byte[] { 1, 2, 3, 4 });

        try
        {
            var application =
                new Application
                {
                    UserId = 1,
                    JobId = 1,
                    CoverLetter = "Application",
                    Status = "Pending",
                    CvFileUrl =
                        fileType == "cv"
                            ? $"/private_uploads/{folderName}/{uniqueFileName}"
                            : null,
                    CertificateFileUrl =
                        fileType == "certificate"
                            ? $"/private_uploads/{folderName}/{uniqueFileName}"
                            : null,
                    PortfolioFileUrl =
                        fileType == "portfolio"
                            ? $"/private_uploads/{folderName}/{uniqueFileName}"
                            : null,
                    CreatedAt = DateTime.UtcNow
                };

            context.Applications.Add(
                application);

            await context.SaveChangesAsync();

            var applicationService =
                CreateApplicationService(
                    context);

            // Act
            var result =
                await applicationService
                    .GetApplicationFileAsync(
                        application.Id,
                        fileType);

            // Assert
            Assert.NotNull(result);

            Assert.Equal(
                Path.GetFullPath(
                    physicalPath),
                result.FilePath);

            Assert.Equal(
                "application/pdf",
                result.ContentType);

            Assert.Equal(
                $"{expectedDownloadPrefix}{application.Id}.pdf",
                result.DownloadFileName);
        }
        finally
        {
            if (File.Exists(physicalPath))
            {
                File.Delete(physicalPath);
            }

            if (Directory.Exists(directory) &&
                !Directory.EnumerateFileSystemEntries(
                    directory).Any())
            {
                Directory.Delete(directory);
            }
        }
    }

    [Fact]
    public async Task GetApplicationFileAsync_ShouldReturnNull_WhenStoredPathIsOutsideAllowedFolders()
    {
        // Arrange
        await using var context =
            TestDbContextFactory.Create();

        var application =
            new Application
            {
                UserId = 1,
                JobId = 1,
                CoverLetter = "Application",
                Status = "Pending",
                CvFileUrl =
                    "/other_folder/test.pdf",
                CreatedAt = DateTime.UtcNow
            };

        context.Applications.Add(application);

        await context.SaveChangesAsync();

        var applicationService =
            CreateApplicationService(
                context);

        // Act
        var result =
            await applicationService
                .GetApplicationFileAsync(
                    application.Id,
                    "cv");

        // Assert
        Assert.Null(result);
    }

    private static ApplicationService CreateApplicationService(
        SkillJobAI.Api.Data.AppDbContext context,
        Mock<IFileStorageService>? fileStorageServiceMock = null,
        Mock<IApplicationMatchingService>? applicationMatchingServiceMock = null,
        Mock<IEmailService>? emailServiceMock = null)
    {
        fileStorageServiceMock ??=
            new Mock<IFileStorageService>();

        applicationMatchingServiceMock ??=
            new Mock<IApplicationMatchingService>();

        emailServiceMock ??=
            new Mock<IEmailService>();

        return new ApplicationService(
            context,
            fileStorageServiceMock.Object,
            applicationMatchingServiceMock.Object,
            emailServiceMock.Object);
    }

    private static Mock<IApplicationMatchingService>
        CreateMatchingServiceMock(
            int jobId,
            int userId)
    {
        var mock =
            new Mock<IApplicationMatchingService>(
                MockBehavior.Strict);

        mock
            .Setup(service =>
                service.GetMatchResultAsync(
                    jobId,
                    userId))
            .ReturnsAsync(
                CreateMatchResult());

        return mock;
    }

    private static ApplicationMatchResult
        CreateMatchResult()
    {
        return new ApplicationMatchResult
        {
            MatchPercentage = 80,
            JobSkills =
                new List<string>
                {
                    "C#",
                    "ASP.NET Core"
                },
            UserSkills =
                new List<string>
                {
                    "C#",
                    "SQL"
                },
            MatchedSkills =
                new List<string>
                {
                    "C#"
                },
            MissingSkills =
                new List<string>
                {
                    "ASP.NET Core"
                },
            RecommendedCourses =
                new List<object>()
        };
    }

    private static AppUser CreateCandidate(
        string fullName = "Test Candidate",
        string email = "candidate@test.com")
    {
        return new AppUser
        {
            FullName = fullName,
            Email = email,
            PasswordHash = "not-relevant-for-test",
            Role = "Candidate",
            CvUrl = "/uploads/candidate-cv.pdf",
            CreatedAt = DateTime.UtcNow
        };
    }

    private static Company CreateCompany()
    {
        return new Company
        {
            Name = "SkillJob Test Company",
            Description = "Test company",
            WebsiteUrl = "https://example.com",
            LogoUrl = "/uploads/company-logo.png",
            Location = "Berlin",
            CreatedAt = DateTime.UtcNow
        };
    }

    private static Job CreateJob(
        Company company,
        string title = "Backend Developer")
    {
        return new Job
        {
            Title = title,
            Description =
                "ASP.NET Core backend position",
            Location = "Berlin",
            Salary = "60.000 - 80.000 EUR",
            Company = company,
            CreatedAt = DateTime.UtcNow
        };
    }

    private static Application CreateApplication(
        int userId,
        int jobId,
        string coverLetter,
        DateTime createdAt)
    {
        return new Application
        {
            UserId = userId,
            JobId = jobId,
            CoverLetter = coverLetter,
            Status = "Pending",
            CreatedAt = createdAt
        };
    }

    private static IFormFile CreateFormFile(
        string fileName)
    {
        var content =
            "%PDF-1.4 test content";

        var stream =
            new MemoryStream(
                System.Text.Encoding.UTF8
                    .GetBytes(content));

        return new FormFile(
            stream,
            0,
            stream.Length,
            "file",
            fileName)
        {
            Headers =
                new HeaderDictionary(),
            ContentType =
                "application/pdf"
        };
    }
}

