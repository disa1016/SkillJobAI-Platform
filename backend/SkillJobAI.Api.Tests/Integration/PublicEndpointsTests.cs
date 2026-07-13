using System.Net;
using System.Net.Http.Json;
using SkillJobAI.Api.Models;

namespace SkillJobAI.Api.Tests.Integration;

public class PublicEndpointsTests
: IClassFixture<CustomWebApplicationFactory>
{
private readonly CustomWebApplicationFactory
_factory;


private readonly HttpClient
    _client;

public PublicEndpointsTests(
    CustomWebApplicationFactory factory)
{
    _factory = factory;

    _client =
        factory.CreateClient();
}

[Fact]
public async Task GetJobs_ShouldReturnOk()
{
    // Arrange
    await _factory.ResetDatabaseAsync();

    // Act
    var response =
        await _client.GetAsync(
            "/api/jobs");

    // Assert
    Assert.Equal(
        HttpStatusCode.OK,
        response.StatusCode);

    var result =
        await response.Content
            .ReadFromJsonAsync<
                PagedResponse<
                    SkillJobAI.Api.Models.Responses.JobResponse>>();

    Assert.NotNull(result);

    Assert.Equal(
        1,
        result.Page);

    Assert.Equal(
        10,
        result.PageSize);

    Assert.Equal(
        0,
        result.TotalItems);

    Assert.Equal(
        0,
        result.TotalPages);

    Assert.Empty(
        result.Items);
}

[Fact]
public async Task GetCompanies_ShouldReturnOk()
{
    // Arrange
    await _factory.ResetDatabaseAsync();

    // Act
    var response =
        await _client.GetAsync(
            "/api/companies");

    // Assert
    Assert.Equal(
        HttpStatusCode.OK,
        response.StatusCode);

    var result =
        await response.Content
            .ReadFromJsonAsync<
                PagedResponse<
                    SkillJobAI.Api.Models.Responses.CompanyResponse>>();

    Assert.NotNull(result);

    Assert.Equal(
        0,
        result.TotalItems);

    Assert.Empty(
        result.Items);
}

[Fact]
public async Task GetJob_ShouldReturnNotFound_WhenJobDoesNotExist()
{
    // Arrange
    await _factory.ResetDatabaseAsync();

    // Act
    var response =
        await _client.GetAsync(
            "/api/jobs/999");

    // Assert
    Assert.Equal(
        HttpStatusCode.NotFound,
        response.StatusCode);
}

[Fact]
public async Task GetCompany_ShouldReturnNotFound_WhenCompanyDoesNotExist()
{
    // Arrange
    await _factory.ResetDatabaseAsync();

    // Act
    var response =
        await _client.GetAsync(
            "/api/companies/999");

    // Assert
    Assert.Equal(
        HttpStatusCode.NotFound,
        response.StatusCode);
}

[Fact]
public async Task GetJobs_ShouldApplyPaginationQueryParameters()
{
    // Arrange
    await _factory.ResetDatabaseAsync();

    // Act
    var response =
        await _client.GetAsync(
            "/api/jobs?page=2&pageSize=5");

    // Assert
    Assert.Equal(
        HttpStatusCode.OK,
        response.StatusCode);

    var result =
        await response.Content
            .ReadFromJsonAsync<
                PagedResponse<
                    SkillJobAI.Api.Models.Responses.JobResponse>>();

    Assert.NotNull(result);

    Assert.Equal(
        2,
        result.Page);

    Assert.Equal(
        5,
        result.PageSize);
}


}
