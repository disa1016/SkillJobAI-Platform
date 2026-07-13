using System.Net;
using System.Net.Http.Json;
using Microsoft.EntityFrameworkCore;
using SkillJobAI.Api.Entities;
using SkillJobAI.Api.Models;

namespace SkillJobAI.Api.Tests.Integration;

public class AuthenticatedAuthorizationTests
: IClassFixture<CustomWebApplicationFactory>
{
private readonly CustomWebApplicationFactory
_factory;

public AuthenticatedAuthorizationTests(
    CustomWebApplicationFactory factory)
{
    _factory = factory;
}

[Fact]
public async Task MyApplications_ShouldReturnOk_ForCandidate()
{
    // Arrange
    await _factory.ResetDatabaseAsync();

    var candidate =
        CreateUser(
            role: "Candidate",
            email: "candidate@test.com");

    await _factory.SeedAsync(
        async context =>
        {
            context.Users.Add(candidate);

            await context.SaveChangesAsync();
        });

    using var client =
        _factory.CreateAuthenticatedClient(
            candidate);

    // Act
    var response =
        await client.GetAsync(
            "/api/applications/my");

    // Assert
    Assert.Equal(
        HttpStatusCode.OK,
        response.StatusCode);
}

[Fact]
public async Task MyApplications_ShouldReturnForbidden_ForRecruiter()
{
    // Arrange
    await _factory.ResetDatabaseAsync();

    var recruiter =
        CreateUser(
            role: "Recruiter",
            email: "recruiter@test.com");

    await _factory.SeedAsync(
        async context =>
        {
            context.Users.Add(recruiter);

            await context.SaveChangesAsync();
        });

    using var client =
        _factory.CreateAuthenticatedClient(
            recruiter);

    // Act
    var response =
        await client.GetAsync(
            "/api/applications/my");

    // Assert
    Assert.Equal(
        HttpStatusCode.Forbidden,
        response.StatusCode);
}

[Fact]
public async Task CreateJob_ShouldReturnForbidden_ForCandidate()
{
    // Arrange
    await _factory.ResetDatabaseAsync();

    var candidate =
        CreateUser(
            role: "Candidate",
            email: "candidate@test.com");

    var company =
        CreateCompany();

    await _factory.SeedAsync(
        async context =>
        {
            context.Users.Add(candidate);
            context.Companies.Add(company);

            await context.SaveChangesAsync();
        });

    using var client =
        _factory.CreateAuthenticatedClient(
            candidate);

    var request =
        CreateJobRequest(
            company.Id);

    // Act
    var response =
        await client.PostAsJsonAsync(
            "/api/jobs",
            request);

    // Assert
    Assert.Equal(
        HttpStatusCode.Forbidden,
        response.StatusCode);
}

[Fact]
public async Task CreateCompany_ShouldReturnForbidden_ForRecruiter()
{
    // Arrange
    await _factory.ResetDatabaseAsync();

    var recruiter =
        CreateUser(
            role: "Recruiter",
            email: "recruiter@test.com");

    await _factory.SeedAsync(
        async context =>
        {
            context.Users.Add(recruiter);

            await context.SaveChangesAsync();
        });

    using var client =
        _factory.CreateAuthenticatedClient(
            recruiter);

    var request =
        new CompanyRequest
        {
            Name = "Recruiter Company",
            Description = "Test company",
            WebsiteUrl = "https://example.com",
            Location = "Berlin"
        };

    // Act
    var response =
        await client.PostAsJsonAsync(
            "/api/companies",
            request);

    // Assert
    Assert.Equal(
        HttpStatusCode.Forbidden,
        response.StatusCode);
}

[Fact]
public async Task CreateCompany_ShouldReturnOk_ForAdmin()
{
    // Arrange
    await _factory.ResetDatabaseAsync();

    var admin =
        CreateUser(
            role: "Admin",
            email: "admin@test.com");

    await _factory.SeedAsync(
        async context =>
        {
            context.Users.Add(admin);

            await context.SaveChangesAsync();
        });

    using var client =
        _factory.CreateAuthenticatedClient(
            admin);

    var request =
        new CompanyRequest
        {
            Name = "Admin Created Company",
            Description = "Created by administrator",
            WebsiteUrl = "https://admin-company.example.com",
            Location = "Berlin"
        };

    // Act
    var response =
        await client.PostAsJsonAsync(
            "/api/companies",
            request);

    // Assert
    Assert.Equal(
        HttpStatusCode.OK,
        response.StatusCode);

    await _factory.SeedAsync(
        async context =>
        {
            var company =
                await context.Companies
                    .SingleAsync();

            Assert.Equal(
                request.Name,
                company.Name);
        });
}

[Fact]
public async Task CreateJob_ShouldReturnOk_WhenRecruiterManagesCompany()
{
    // Arrange
    await _factory.ResetDatabaseAsync();

    var recruiter =
        CreateUser(
            role: "Recruiter",
            email: "owner@test.com");

    var company =
        CreateCompany();

    await _factory.SeedAsync(
        async context =>
        {
            context.Users.Add(recruiter);
            context.Companies.Add(company);

            await context.SaveChangesAsync();

            context.CompanyMembers.Add(
                new CompanyMember
                {
                    UserId = recruiter.Id,
                    CompanyId = company.Id,
                    Role = "Recruiter",
                    JoinedAt = DateTime.UtcNow
                });

            await context.SaveChangesAsync();
        });

    using var client =
        _factory.CreateAuthenticatedClient(
            recruiter);

    var request =
        CreateJobRequest(
            company.Id);

    // Act
    var response =
        await client.PostAsJsonAsync(
            "/api/jobs",
            request);

    // Assert
    Assert.Equal(
        HttpStatusCode.OK,
        response.StatusCode);

    await _factory.SeedAsync(
        async context =>
        {
            var job =
                await context.Jobs
                    .SingleAsync();

            Assert.Equal(
                request.Title,
                job.Title);

            Assert.Equal(
                company.Id,
                job.CompanyId);
        });
}

[Fact]
public async Task CreateJob_ShouldReturnForbidden_WhenRecruiterDoesNotManageCompany()
{
    // Arrange
    await _factory.ResetDatabaseAsync();

    var recruiter =
        CreateUser(
            role: "Recruiter",
            email: "foreign@test.com");

    var company =
        CreateCompany();

    await _factory.SeedAsync(
        async context =>
        {
            context.Users.Add(recruiter);
            context.Companies.Add(company);

            await context.SaveChangesAsync();
        });

    using var client =
        _factory.CreateAuthenticatedClient(
            recruiter);

    var request =
        CreateJobRequest(
            company.Id);

    // Act
    var response =
        await client.PostAsJsonAsync(
            "/api/jobs",
            request);

    // Assert
    Assert.Equal(
        HttpStatusCode.Forbidden,
        response.StatusCode);
}

[Fact]
public async Task CreateJob_ShouldReturnOk_ForAdmin()
{
    // Arrange
    await _factory.ResetDatabaseAsync();

    var admin =
        CreateUser(
            role: "Admin",
            email: "admin@test.com");

    var company =
        CreateCompany();

    await _factory.SeedAsync(
        async context =>
        {
            context.Users.Add(admin);
            context.Companies.Add(company);

            await context.SaveChangesAsync();
        });

    using var client =
        _factory.CreateAuthenticatedClient(
            admin);

    var request =
        CreateJobRequest(
            company.Id);

    // Act
    var response =
        await client.PostAsJsonAsync(
            "/api/jobs",
            request);

    // Assert
    Assert.Equal(
        HttpStatusCode.OK,
        response.StatusCode);
}

[Fact]
public async Task DeleteJob_ShouldReturnForbidden_WhenRecruiterDoesNotManageCompany()
{
    // Arrange
    await _factory.ResetDatabaseAsync();

    var recruiter =
        CreateUser(
            role: "Recruiter",
            email: "recruiter@test.com");

    var company =
        CreateCompany();

    var job =
        new Job
        {
            Title = "Backend Developer",
            Description = "ASP.NET Core position",
            Location = "Berlin",
            Salary = "70.000 EUR",
            Company = company,
            CreatedAt = DateTime.UtcNow
        };

    await _factory.SeedAsync(
        async context =>
        {
            context.Users.Add(recruiter);
            context.Companies.Add(company);
            context.Jobs.Add(job);

            await context.SaveChangesAsync();
        });

    using var client =
        _factory.CreateAuthenticatedClient(
            recruiter);

    // Act
    var response =
        await client.DeleteAsync(
            $"/api/jobs/{job.Id}");

    // Assert
    Assert.Equal(
        HttpStatusCode.Forbidden,
        response.StatusCode);
}

[Fact]
public async Task DeleteJob_ShouldReturnNoContent_WhenRecruiterManagesCompany()
{
    // Arrange
    await _factory.ResetDatabaseAsync();

    var recruiter =
        CreateUser(
            role: "Recruiter",
            email: "recruiter@test.com");

    var company =
        CreateCompany();

    var job =
        new Job
        {
            Title = "Backend Developer",
            Description = "ASP.NET Core position",
            Location = "Berlin",
            Salary = "70.000 EUR",
            Company = company,
            CreatedAt = DateTime.UtcNow
        };

    await _factory.SeedAsync(
        async context =>
        {
            context.Users.Add(recruiter);
            context.Companies.Add(company);
            context.Jobs.Add(job);

            await context.SaveChangesAsync();

            context.CompanyMembers.Add(
                new CompanyMember
                {
                    UserId = recruiter.Id,
                    CompanyId = company.Id,
                    Role = "Recruiter",
                    JoinedAt = DateTime.UtcNow
                });

            await context.SaveChangesAsync();
        });

    using var client =
        _factory.CreateAuthenticatedClient(
            recruiter);

    // Act
    var response =
        await client.DeleteAsync(
            $"/api/jobs/{job.Id}");

    // Assert
    Assert.Equal(
        HttpStatusCode.NoContent,
        response.StatusCode);

    await _factory.SeedAsync(
        async context =>
        {
            Assert.Empty(
                await context.Jobs
                    .ToListAsync());
        });
}

private static AppUser CreateUser(
    string role,
    string email)
{
    return new AppUser
    {
        FullName = $"{role} Test User",
        Email = email,
        PasswordHash = "not-relevant-for-jwt-test",
        Role = role,
        CreatedAt = DateTime.UtcNow
    };
}

private static Company CreateCompany()
{
    return new Company
    {
        Name = "Integration Test Company",
        Description = "Company for integration tests",
        WebsiteUrl = "https://example.com",
        Location = "Berlin",
        CreatedAt = DateTime.UtcNow
    };
}

private static JobRequest CreateJobRequest(
    int companyId)
{
    return new JobRequest
    {
        Title = "Senior Backend Developer",
        Description =
            "Develop ASP.NET Core Web APIs.",
        Location = "Berlin",
        Salary = "70.000 - 90.000 EUR",
        CompanyId = companyId
    };
}

}
