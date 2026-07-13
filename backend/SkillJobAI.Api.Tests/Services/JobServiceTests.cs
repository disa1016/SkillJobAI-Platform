using Microsoft.EntityFrameworkCore;
using SkillJobAI.Api.Entities;
using SkillJobAI.Api.Models;
using SkillJobAI.Api.Services;
using SkillJobAI.Api.Tests.Helpers;


namespace SkillJobAI.Api.Tests.Services;

public class JobServiceTests
{
    [Fact]
    public async Task GetJobsAsync_ShouldReturnPagedJobs()
    {
        // Arrange
        await using var context =
        TestDbContextFactory.Create();

        var company = CreateCompany();

        context.Companies.Add(company);

        await context.SaveChangesAsync();

        context.Jobs.AddRange(
            CreateJob(
                company.Id,
                "First Job",
                DateTime.UtcNow.AddMinutes(-3)),
            CreateJob(
                company.Id,
                "Second Job",
                DateTime.UtcNow.AddMinutes(-2)),
            CreateJob(
                company.Id,
                "Third Job",
                DateTime.UtcNow.AddMinutes(-1)));

        await context.SaveChangesAsync();

        var jobService =
            new JobService(context);

        // Act
        var result =
            await jobService.GetJobsAsync(
                page: 1,
                pageSize: 2,
                search: null);

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
            "Third Job",
            result.Items[0].Title);

        Assert.Equal(
            "Second Job",
            result.Items[1].Title);
    }

    [Fact]
    public async Task GetJobsAsync_ShouldReturnSecondPage()
    {
        // Arrange
        await using var context =
            TestDbContextFactory.Create();

        var company = CreateCompany();

        context.Companies.Add(company);

        await context.SaveChangesAsync();

        context.Jobs.AddRange(
            CreateJob(
                company.Id,
                "First Job",
                DateTime.UtcNow.AddMinutes(-3)),
            CreateJob(
                company.Id,
                "Second Job",
                DateTime.UtcNow.AddMinutes(-2)),
            CreateJob(
                company.Id,
                "Third Job",
                DateTime.UtcNow.AddMinutes(-1)));

        await context.SaveChangesAsync();

        var jobService =
            new JobService(context);

        // Act
        var result =
            await jobService.GetJobsAsync(
                page: 2,
                pageSize: 2,
                search: null);

        // Assert
        Assert.Equal(
            2,
            result.Page);

        Assert.Single(
            result.Items);

        Assert.Equal(
            "First Job",
            result.Items[0].Title);
    }


    [Fact]
    public async Task GetJobsAsync_ShouldFilterByTitle()
    {
        // Arrange
        await using var context =
            TestDbContextFactory.Create();

        var company = CreateCompany();

        context.Companies.Add(company);

        await context.SaveChangesAsync();

        var backendJob =
            CreateJob(
                company.Id,
                "Backend Developer");

        backendJob.Description =
            "Build server-side applications.";

        var frontendJob =
            CreateJob(
                company.Id,
                "Frontend Developer");

        frontendJob.Description =
            "Build Vue user interfaces.";

        context.Jobs.AddRange(
            backendJob,
            frontendJob);

        await context.SaveChangesAsync();

        var jobService =
            new JobService(context);

        // Act
        var result =
            await jobService.GetJobsAsync(
                page: 1,
                pageSize: 10,
                search: "backend");

        // Assert
        var job =
            Assert.Single(result.Items);

        Assert.Equal(
            backendJob.Id,
            job.Id);

        Assert.Equal(
            "Backend Developer",
            job.Title);

        Assert.Equal(
            1,
            result.TotalItems);
    }



    [Fact]
    public async Task GetJobsAsync_ShouldFilterByDescription()
    {
        // Arrange
        await using var context =
            TestDbContextFactory.Create();

        var company = CreateCompany();

        context.Companies.Add(company);

        await context.SaveChangesAsync();

        var backendJob =
            CreateJob(
                company.Id,
                "Software Engineer");

        backendJob.Description =
            "Develop REST APIs with ASP.NET Core.";

        var frontendJob =
            CreateJob(
                company.Id,
                "Web Developer");

        frontendJob.Description =
            "Build Vue applications.";

        context.Jobs.AddRange(
            backendJob,
            frontendJob);

        await context.SaveChangesAsync();

        var jobService =
            new JobService(context);

        // Act
        var result =
            await jobService.GetJobsAsync(
                page: 1,
                pageSize: 10,
                search: "asp.net");

        // Assert
        Assert.Single(
            result.Items);

        Assert.Equal(
            backendJob.Title,
            result.Items[0].Title);
    }

    [Fact]
    public async Task GetJobsAsync_ShouldFilterByLocation()
    {
        // Arrange
        await using var context =
            TestDbContextFactory.Create();

        var company = CreateCompany();

        context.Companies.Add(company);

        await context.SaveChangesAsync();

        var berlinJob =
            CreateJob(
                company.Id,
                "Berlin Job");

        berlinJob.Location =
            "Berlin";

        var hamburgJob =
            CreateJob(
                company.Id,
                "Hamburg Job");

        hamburgJob.Location =
            "Hamburg";

        context.Jobs.AddRange(
            berlinJob,
            hamburgJob);

        await context.SaveChangesAsync();

        var jobService =
            new JobService(context);

        // Act
        var result =
            await jobService.GetJobsAsync(
                page: 1,
                pageSize: 10,
                search: "berlin");

        // Assert
        Assert.Single(
            result.Items);

        Assert.Equal(
            "Berlin",
            result.Items[0].Location);
    }

    [Fact]
    public async Task GetJobsAsync_ShouldFilterByCompanyName()
    {
        // Arrange
        await using var context =
            TestDbContextFactory.Create();

        var firstCompany =
            CreateCompany(
                "SkillJob Technologies");

        var secondCompany =
            CreateCompany(
                "Other Company");

        context.Companies.AddRange(
            firstCompany,
            secondCompany);

        await context.SaveChangesAsync();

        context.Jobs.AddRange(
            CreateJob(
                firstCompany.Id,
                "Backend Developer"),
            CreateJob(
                secondCompany.Id,
                "Frontend Developer"));

        await context.SaveChangesAsync();

        var jobService =
            new JobService(context);

        // Act
        var result =
            await jobService.GetJobsAsync(
                page: 1,
                pageSize: 10,
                search: "skilljob");

        // Assert
        Assert.Single(
            result.Items);

        Assert.Equal(
            firstCompany.Name,
            result.Items[0].CompanyName);
    }

    [Fact]
    public async Task GetJobsAsync_ShouldNormalizeInvalidPagination()
    {
        // Arrange
        await using var context =
            TestDbContextFactory.Create();

        var jobService =
            new JobService(context);

        // Act
        var result =
            await jobService.GetJobsAsync(
                page: 0,
                pageSize: 0,
                search: null);

        // Assert
        Assert.Equal(
            1,
            result.Page);

        Assert.Equal(
            10,
            result.PageSize);

        Assert.Empty(
            result.Items);
    }

    [Fact]
    public async Task GetJobsAsync_ShouldLimitPageSizeToFifty()
    {
        // Arrange
        await using var context =
            TestDbContextFactory.Create();

        var jobService =
            new JobService(context);

        // Act
        var result =
            await jobService.GetJobsAsync(
                page: 1,
                pageSize: 100,
                search: null);

        // Assert
        Assert.Equal(
            50,
            result.PageSize);
    }

    [Fact]
    public async Task GetJobByIdAsync_ShouldReturnNull_WhenJobDoesNotExist()
    {
        // Arrange
        await using var context =
            TestDbContextFactory.Create();

        var jobService =
            new JobService(context);

        // Act
        var result =
            await jobService.GetJobByIdAsync(
                id: 999);

        // Assert
        Assert.Null(
            result);
    }

    [Fact]
    public async Task GetJobByIdAsync_ShouldReturnJob_WhenJobExists()
    {
        // Arrange
        await using var context =
            TestDbContextFactory.Create();

        var company =
            CreateCompany();

        context.Companies.Add(
            company);

        await context.SaveChangesAsync();

        var job =
            CreateJob(
                company.Id,
                "Senior Backend Developer");

        context.Jobs.Add(
            job);

        await context.SaveChangesAsync();

        var jobService =
            new JobService(context);

        // Act
        var result =
            await jobService.GetJobByIdAsync(
                job.Id);

        // Assert
        Assert.NotNull(
            result);

        Assert.Equal(
            job.Id,
            result.Id);

        Assert.Equal(
            job.Title,
            result.Title);

        Assert.Equal(
            job.Description,
            result.Description);

        Assert.Equal(
            job.Location,
            result.Location);

        Assert.Equal(
            job.Salary,
            result.Salary);

        Assert.Equal(
            company.Id,
            result.CompanyId);

        Assert.Equal(
            company.Name,
            result.CompanyName);

        Assert.NotNull(
            result.Company);

        Assert.Equal(
            company.Name,
            result.Company.Name);

        Assert.Equal(
            company.Description,
            result.Company.Description);

        Assert.Equal(
            company.WebsiteUrl,
            result.Company.WebsiteUrl);

        Assert.Equal(
            company.LogoUrl,
            result.Company.LogoUrl);

        Assert.Equal(
            company.Location,
            result.Company.Location);
    }

    [Fact]
    public async Task CreateJobAsync_ShouldReturnNull_WhenCompanyIdIsNull()
    {
        // Arrange
        await using var context =
            TestDbContextFactory.Create();

        var jobService =
            new JobService(context);

        var request =
            CreateJobRequest(
                companyId: null);

        // Act
        var result =
            await jobService.CreateJobAsync(
                request);

        // Assert
        Assert.Null(
            result);

        Assert.Empty(
            await context.Jobs
                .ToListAsync());
    }

    [Fact]
    public async Task CreateJobAsync_ShouldReturnNull_WhenCompanyDoesNotExist()
    {
        // Arrange
        await using var context =
            TestDbContextFactory.Create();

        var jobService =
            new JobService(context);

        var request =
            CreateJobRequest(
                companyId: 999);

        // Act
        var result =
            await jobService.CreateJobAsync(
                request);

        // Assert
        Assert.Null(
            result);

        Assert.Empty(
            await context.Jobs
                .ToListAsync());
    }

    [Fact]
    public async Task CreateJobAsync_ShouldCreateJob_WhenCompanyExists()
    {
        // Arrange
        await using var context =
            TestDbContextFactory.Create();

        var company =
            CreateCompany();

        context.Companies.Add(
            company);

        await context.SaveChangesAsync();

        var jobService =
            new JobService(context);

        var request =
            CreateJobRequest(
                company.Id);

        var beforeCreation =
            DateTime.UtcNow;

        // Act
        var result =
            await jobService.CreateJobAsync(
                request);

        var afterCreation =
            DateTime.UtcNow;

        // Assert
        Assert.NotNull(
            result);

        var storedJob =
            await context.Jobs
                .SingleAsync();

        Assert.Equal(
            request.Title,
            storedJob.Title);

        Assert.Equal(
            request.Description,
            storedJob.Description);

        Assert.Equal(
            request.Location,
            storedJob.Location);

        Assert.Equal(
            request.Salary,
            storedJob.Salary);

        Assert.Equal(
            company.Id,
            storedJob.CompanyId);

        Assert.InRange(
            storedJob.CreatedAt,
            beforeCreation,
            afterCreation);

        Assert.Equal(
            storedJob.Id,
            result.Id);

        Assert.Equal(
            company.Name,
            result.CompanyName);

        Assert.NotNull(
            result.Company);

        Assert.Equal(
            company.Id,
            result.Company.Id);

        Assert.Equal(
            company.Name,
            result.Company.Name);
    }

    [Fact]
    public async Task UpdateJobAsync_ShouldReturnNull_WhenJobDoesNotExist()
    {
        // Arrange
        await using var context =
            TestDbContextFactory.Create();

        var company =
            CreateCompany();

        context.Companies.Add(
            company);

        await context.SaveChangesAsync();

        var jobService =
            new JobService(context);

        var request =
            CreateJobRequest(
                company.Id);

        // Act
        var result =
            await jobService.UpdateJobAsync(
                id: 999,
                request);

        // Assert
        Assert.Null(
            result);
    }

    [Fact]
    public async Task UpdateJobAsync_ShouldReturnNull_WhenCompanyIdIsNull()
    {
        // Arrange
        await using var context =
            TestDbContextFactory.Create();

        var company =
            CreateCompany();

        context.Companies.Add(
            company);

        await context.SaveChangesAsync();

        var job =
            CreateJob(
                company.Id);

        context.Jobs.Add(
            job);

        await context.SaveChangesAsync();

        var jobService =
            new JobService(context);

        var request =
            CreateJobRequest(
                companyId: null);

        // Act
        var result =
            await jobService.UpdateJobAsync(
                job.Id,
                request);

        // Assert
        Assert.Null(
            result);

        var storedJob =
            await context.Jobs
                .SingleAsync();

        Assert.Equal(
            job.Title,
            storedJob.Title);
    }

    [Fact]
    public async Task UpdateJobAsync_ShouldReturnNull_WhenCompanyDoesNotExist()
    {
        // Arrange
        await using var context =
            TestDbContextFactory.Create();

        var company =
            CreateCompany();

        context.Companies.Add(
            company);

        await context.SaveChangesAsync();

        var job =
            CreateJob(
                company.Id);

        context.Jobs.Add(
            job);

        await context.SaveChangesAsync();

        var originalTitle =
            job.Title;

        var jobService =
            new JobService(context);

        var request =
            CreateJobRequest(
                companyId: 999);

        request.Title =
            "Updated title";

        // Act
        var result =
            await jobService.UpdateJobAsync(
                job.Id,
                request);

        // Assert
        Assert.Null(
            result);

        var storedJob =
            await context.Jobs
                .SingleAsync();

        Assert.Equal(
            originalTitle,
            storedJob.Title);

        Assert.Equal(
            company.Id,
            storedJob.CompanyId);
    }

    [Fact]
    public async Task UpdateJobAsync_ShouldUpdateJob_WhenDataIsValid()
    {
        // Arrange
        await using var context =
            TestDbContextFactory.Create();

        var oldCompany =
            CreateCompany(
                "Old Company");

        var newCompany =
            CreateCompany(
                "New Company");

        context.Companies.AddRange(
            oldCompany,
            newCompany);

        await context.SaveChangesAsync();

        var job =
            CreateJob(
                oldCompany.Id,
                "Old Job Title");

        var originalCreatedAt =
            job.CreatedAt;

        context.Jobs.Add(
            job);

        await context.SaveChangesAsync();

        var jobService =
            new JobService(context);

        var request =
            new JobRequest
            {
                Title =
                    "Updated Backend Developer",
                Description =
                    "Updated job description",
                Location =
                    "Munich",
                Salary =
                    "80.000 - 100.000 EUR",
                CompanyId =
                    newCompany.Id
            };

        // Act
        var result =
            await jobService.UpdateJobAsync(
                job.Id,
                request);

        // Assert
        Assert.NotNull(
            result);

        var storedJob =
            await context.Jobs
                .SingleAsync();

        Assert.Equal(
            request.Title,
            storedJob.Title);

        Assert.Equal(
            request.Description,
            storedJob.Description);

        Assert.Equal(
            request.Location,
            storedJob.Location);

        Assert.Equal(
            request.Salary,
            storedJob.Salary);

        Assert.Equal(
            newCompany.Id,
            storedJob.CompanyId);

        Assert.Equal(
            originalCreatedAt,
            storedJob.CreatedAt);

        Assert.Equal(
            newCompany.Name,
            result.CompanyName);

        Assert.NotNull(
            result.Company);

        Assert.Equal(
            newCompany.Id,
            result.Company.Id);
    }

    [Fact]
    public async Task DeleteJobAsync_ShouldReturnFalse_WhenJobDoesNotExist()
    {
        // Arrange
        await using var context =
            TestDbContextFactory.Create();

        var jobService =
            new JobService(context);

        // Act
        var result =
            await jobService.DeleteJobAsync(
                id: 999);

        // Assert
        Assert.False(
            result);
    }

    [Fact]
    public async Task DeleteJobAsync_ShouldDeleteJob_WhenJobExists()
    {
        // Arrange
        await using var context =
            TestDbContextFactory.Create();

        var company =
            CreateCompany();

        context.Companies.Add(
            company);

        await context.SaveChangesAsync();

        var job =
            CreateJob(
                company.Id);

        context.Jobs.Add(
            job);

        await context.SaveChangesAsync();

        var jobService =
            new JobService(context);

        // Act
        var result =
            await jobService.DeleteJobAsync(
                job.Id);

        // Assert
        Assert.True(
            result);

        Assert.Empty(
            await context.Jobs
                .ToListAsync());
    }

    [Fact]
    public async Task CompanyExistsAsync_ShouldReturnTrue_WhenCompanyExists()
    {
        // Arrange
        await using var context =
            TestDbContextFactory.Create();

        var company =
            CreateCompany();

        context.Companies.Add(
            company);

        await context.SaveChangesAsync();

        var jobService =
            new JobService(context);

        // Act
        var result =
            await jobService.CompanyExistsAsync(
                company.Id);

        // Assert
        Assert.True(
            result);
    }

    [Fact]
    public async Task CompanyExistsAsync_ShouldReturnFalse_WhenCompanyDoesNotExist()
    {
        // Arrange
        await using var context =
            TestDbContextFactory.Create();

        var jobService =
            new JobService(context);

        // Act
        var result =
            await jobService.CompanyExistsAsync(
                companyId: 999);

        // Assert
        Assert.False(
            result);
    }

    [Fact]
    public async Task GetJobCompanyIdAsync_ShouldReturnCompanyId_WhenJobExists()
    {
        // Arrange
        await using var context =
            TestDbContextFactory.Create();

        var company =
            CreateCompany();

        context.Companies.Add(
            company);

        await context.SaveChangesAsync();

        var job =
            CreateJob(
                company.Id);

        context.Jobs.Add(
            job);

        await context.SaveChangesAsync();

        var jobService =
            new JobService(context);

        // Act
        var result =
            await jobService.GetJobCompanyIdAsync(
                job.Id);

        // Assert
        Assert.Equal(
            company.Id,
            result);
    }

    [Fact]
    public async Task GetJobCompanyIdAsync_ShouldReturnNull_WhenJobDoesNotExist()
    {
        // Arrange
        await using var context =
            TestDbContextFactory.Create();

        var jobService =
            new JobService(context);

        // Act
        var result =
            await jobService.GetJobCompanyIdAsync(
                jobId: 999);

        // Assert
        Assert.Null(
            result);
    }

    [Fact]
    public async Task GetJobCompanyIdAsync_ShouldReturnNull_WhenJobHasNoCompany()
    {
        // Arrange
        await using var context =
            TestDbContextFactory.Create();

        var job =
            new Job
            {
                Title =
                    "Independent Job",
                Description =
                    "Job without company",
                Location =
                    "Remote",
                Salary =
                    "Negotiable",
                CompanyId =
                    null,
                CreatedAt =
                    DateTime.UtcNow
            };

        context.Jobs.Add(
            job);

        await context.SaveChangesAsync();

        var jobService =
            new JobService(context);

        // Act
        var result =
            await jobService.GetJobCompanyIdAsync(
                job.Id);

        // Assert
        Assert.Null(
            result);
    }

    private static Company CreateCompany(
        string name = "SkillJob Test Company")
    {
        return new Company
        {
            Name =
                name,
            Description =
                "Test company description",
            WebsiteUrl =
                "https://example.com",
            LogoUrl =
                "/uploads/company-logo.png",
            Location =
                "Berlin",
            CreatedAt =
                DateTime.UtcNow
        };
    }

    private static Job CreateJob(
        int companyId,
        string title = "Backend Developer",
        DateTime? createdAt = null)
    {
        return new Job
        {
            Title =
                title,
            Description =
                "ASP.NET Core backend position",
            Location =
                "Berlin",
            Salary =
                "60.000 - 80.000 EUR",
            CompanyId =
                companyId,
            CreatedAt =
                createdAt ?? DateTime.UtcNow
        };
    }

    private static JobRequest CreateJobRequest(
        int? companyId)
    {
        return new JobRequest
        {
            Title =
                "Senior Backend Developer",
            Description =
                "Develop and maintain ASP.NET Core APIs.",
            Location =
                "Berlin",
            Salary =
                "70.000 - 90.000 EUR",
            CompanyId =
                companyId
        };
    }


}
