using System.Net;
using System.Net.Http.Json;
using SkillJobAI.Api.Models;

namespace SkillJobAI.Api.Tests.Integration;

public class AuthorizationTests
: IClassFixture<CustomWebApplicationFactory>
{
private readonly CustomWebApplicationFactory
_factory;

private readonly HttpClient
    _client;

public AuthorizationTests(
    CustomWebApplicationFactory factory)
{
    _factory = factory;

    _client =
        factory.CreateClient();
}

[Fact]
public async Task CreateCompany_ShouldReturnUnauthorized_WhenTokenIsMissing()
{
    // Arrange
    await _factory.ResetDatabaseAsync();

    var request =
        new CompanyRequest
        {
            Name = "Unauthorized Company",
            Description = "Test company",
            WebsiteUrl =
                "https://example.com",
            Location = "Berlin"
        };

    // Act
    var response =
        await _client.PostAsJsonAsync(
            "/api/companies",
            request);

    // Assert
    Assert.Equal(
        HttpStatusCode.Unauthorized,
        response.StatusCode);
}

[Fact]
public async Task UpdateCompany_ShouldReturnUnauthorized_WhenTokenIsMissing()
{
    // Arrange
    await _factory.ResetDatabaseAsync();

    var request =
        new CompanyRequest
        {
            Name = "Updated Company",
            Description = "Updated description",
            WebsiteUrl =
                "https://example.com",
            Location = "Berlin"
        };

    // Act
    var response =
        await _client.PutAsJsonAsync(
            "/api/companies/1",
            request);

    // Assert
    Assert.Equal(
        HttpStatusCode.Unauthorized,
        response.StatusCode);
}

[Fact]
public async Task DeleteCompany_ShouldReturnUnauthorized_WhenTokenIsMissing()
{
    // Arrange
    await _factory.ResetDatabaseAsync();

    // Act
    var response =
        await _client.DeleteAsync(
            "/api/companies/1");

    // Assert
    Assert.Equal(
        HttpStatusCode.Unauthorized,
        response.StatusCode);
}

[Fact]
public async Task CreateJob_ShouldReturnUnauthorized_WhenTokenIsMissing()
{
    // Arrange
    await _factory.ResetDatabaseAsync();

    var request =
        new JobRequest
        {
            Title = "Backend Developer",
            Description =
                "ASP.NET Core backend position",
            Location = "Berlin",
            Salary = "70.000 EUR",
            CompanyId = 1
        };

    // Act
    var response =
        await _client.PostAsJsonAsync(
            "/api/jobs",
            request);

    // Assert
    Assert.Equal(
        HttpStatusCode.Unauthorized,
        response.StatusCode);
}

[Fact]
public async Task UpdateJob_ShouldReturnUnauthorized_WhenTokenIsMissing()
{
    // Arrange
    await _factory.ResetDatabaseAsync();

    var request =
        new JobRequest
        {
            Title = "Updated Job",
            Description =
                "Updated description",
            Location = "Hamburg",
            Salary = "80.000 EUR",
            CompanyId = 1
        };

    // Act
    var response =
        await _client.PutAsJsonAsync(
            "/api/jobs/1",
            request);

    // Assert
    Assert.Equal(
        HttpStatusCode.Unauthorized,
        response.StatusCode);
}

[Fact]
public async Task DeleteJob_ShouldReturnUnauthorized_WhenTokenIsMissing()
{
    // Arrange
    await _factory.ResetDatabaseAsync();

    // Act
    var response =
        await _client.DeleteAsync(
            "/api/jobs/1");

    // Assert
    Assert.Equal(
        HttpStatusCode.Unauthorized,
        response.StatusCode);
}

[Fact]
public async Task MyApplications_ShouldReturnUnauthorized_WhenTokenIsMissing()
{
    // Arrange
    await _factory.ResetDatabaseAsync();

    // Act
    var response =
        await _client.GetAsync(
            "/api/applications/my");

    // Assert
    Assert.Equal(
        HttpStatusCode.Unauthorized,
        response.StatusCode);
}

[Fact]
public async Task GetApplicationsForJob_ShouldReturnUnauthorized_WhenTokenIsMissing()
{
    // Arrange
    await _factory.ResetDatabaseAsync();

    // Act
    var response =
        await _client.GetAsync(
            "/api/applications/job/1");

    // Assert
    Assert.Equal(
        HttpStatusCode.Unauthorized,
        response.StatusCode);
}

[Fact]
public async Task UpdateApplicationStatus_ShouldReturnUnauthorized_WhenTokenIsMissing()
{
    // Arrange
    await _factory.ResetDatabaseAsync();

    var request =
        new
        {
            Status = "Accepted"
        };

    // Act
    var response =
        await _client.PutAsJsonAsync(
            "/api/applications/1/status",
            request);

    // Assert
    Assert.Equal(
        HttpStatusCode.Unauthorized,
        response.StatusCode);
}

}
