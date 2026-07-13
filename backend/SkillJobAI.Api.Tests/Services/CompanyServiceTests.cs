using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using SkillJobAI.Api.Entities;
using SkillJobAI.Api.Models;
using SkillJobAI.Api.Services;
using SkillJobAI.Api.Tests.Helpers;
using System.Text;

namespace SkillJobAI.Api.Tests.Services;

public class CompanyServiceTests
{
[Fact]
public async Task GetCompaniesAsync_ShouldReturnPagedCompanies()
{
// Arrange
await using var context =
TestDbContextFactory.Create();

    context.Companies.AddRange(
        CreateCompany(
            "Alpha Company",
            DateTime.UtcNow.AddMinutes(-3)),
        CreateCompany(
            "Beta Company",
            DateTime.UtcNow.AddMinutes(-2)),
        CreateCompany(
            "Gamma Company",
            DateTime.UtcNow.AddMinutes(-1)));

    await context.SaveChangesAsync();

    var companyService =
        new CompanyService(context);

    // Act
    var result =
        await companyService.GetCompaniesAsync(
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
        "Alpha Company",
        result.Items[0].Name);

    Assert.Equal(
        "Beta Company",
        result.Items[1].Name);
}

[Fact]
public async Task GetCompaniesAsync_ShouldReturnSecondPage()
{
    // Arrange
    await using var context =
        TestDbContextFactory.Create();

    context.Companies.AddRange(
        CreateCompany("Alpha Company"),
        CreateCompany("Beta Company"),
        CreateCompany("Gamma Company"));

    await context.SaveChangesAsync();

    var companyService =
        new CompanyService(context);

    // Act
    var result =
        await companyService.GetCompaniesAsync(
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
        "Gamma Company",
        result.Items[0].Name);
}

[Fact]
public async Task GetCompaniesAsync_ShouldFilterByName()
{
    // Arrange
    await using var context =
        TestDbContextFactory.Create();

    context.Companies.AddRange(
        CreateCompany("SkillJob Technologies"),
        CreateCompany("Other Corporation"));

    await context.SaveChangesAsync();

    var companyService =
        new CompanyService(context);

    // Act
    var result =
        await companyService.GetCompaniesAsync(
            page: 1,
            pageSize: 10,
            search: "skilljob");

    // Assert
    var company =
        Assert.Single(result.Items);

    Assert.Equal(
        "SkillJob Technologies",
        company.Name);

    Assert.Equal(
        1,
        result.TotalItems);
}

[Fact]
public async Task GetCompaniesAsync_ShouldFilterByDescription()
{
    // Arrange
    await using var context =
        TestDbContextFactory.Create();

    var aiCompany =
        CreateCompany("AI Company");

    aiCompany.Description =
        "Artificial intelligence recruitment platform";

    var logisticsCompany =
        CreateCompany("Logistics Company");

    logisticsCompany.Description =
        "Transport and warehouse services";

    context.Companies.AddRange(
        aiCompany,
        logisticsCompany);

    await context.SaveChangesAsync();

    var companyService =
        new CompanyService(context);

    // Act
    var result =
        await companyService.GetCompaniesAsync(
            page: 1,
            pageSize: 10,
            search: "recruitment");

    // Assert
    var company =
        Assert.Single(result.Items);

    Assert.Equal(
        aiCompany.Id,
        company.Id);
}

[Fact]
public async Task GetCompaniesAsync_ShouldFilterByLocation()
{
    // Arrange
    await using var context =
        TestDbContextFactory.Create();

    var berlinCompany =
        CreateCompany("Berlin Company");

    berlinCompany.Location =
        "Berlin";

    var munichCompany =
        CreateCompany("Munich Company");

    munichCompany.Location =
        "Munich";

    context.Companies.AddRange(
        berlinCompany,
        munichCompany);

    await context.SaveChangesAsync();

    var companyService =
        new CompanyService(context);

    // Act
    var result =
        await companyService.GetCompaniesAsync(
            page: 1,
            pageSize: 10,
            search: "berlin");

    // Assert
    var company =
        Assert.Single(result.Items);

    Assert.Equal(
        "Berlin",
        company.Location);
}

[Fact]
public async Task GetCompaniesAsync_ShouldFilterByWebsiteUrl()
{
    // Arrange
    await using var context =
        TestDbContextFactory.Create();

    var skillJobCompany =
        CreateCompany("SkillJob Company");

    skillJobCompany.WebsiteUrl =
        "https://skilljob.example.com";

    var otherCompany =
        CreateCompany("Other Company");

    otherCompany.WebsiteUrl =
        "https://other.example.com";

    context.Companies.AddRange(
        skillJobCompany,
        otherCompany);

    await context.SaveChangesAsync();

    var companyService =
        new CompanyService(context);

    // Act
    var result =
        await companyService.GetCompaniesAsync(
            page: 1,
            pageSize: 10,
            search: "skilljob.example");

    // Assert
    var company =
        Assert.Single(result.Items);

    Assert.Equal(
        skillJobCompany.Id,
        company.Id);
}

[Fact]
public async Task GetCompaniesAsync_ShouldNormalizeInvalidPagination()
{
    // Arrange
    await using var context =
        TestDbContextFactory.Create();

    var companyService =
        new CompanyService(context);

    // Act
    var result =
        await companyService.GetCompaniesAsync(
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
public async Task GetCompaniesAsync_ShouldLimitPageSizeToFifty()
{
    // Arrange
    await using var context =
        TestDbContextFactory.Create();

    var companyService =
        new CompanyService(context);

    // Act
    var result =
        await companyService.GetCompaniesAsync(
            page: 1,
            pageSize: 100,
            search: null);

    // Assert
    Assert.Equal(
        50,
        result.PageSize);
}

[Fact]
public async Task GetCompaniesAsync_ShouldReturnTotalJobs()
{
    // Arrange
    await using var context =
        TestDbContextFactory.Create();

    var company =
        CreateCompany();

    context.Companies.Add(company);

    await context.SaveChangesAsync();

    context.Jobs.AddRange(
        CreateJob(
            company.Id,
            "Backend Developer"),
        CreateJob(
            company.Id,
            "Frontend Developer"));

    await context.SaveChangesAsync();

    var companyService =
        new CompanyService(context);

    // Act
    var result =
        await companyService.GetCompaniesAsync(
            page: 1,
            pageSize: 10,
            search: null);

    // Assert
    var returnedCompany =
        Assert.Single(result.Items);

    Assert.Equal(
        2,
        returnedCompany.TotalJobs);
}

[Fact]
public async Task GetCompanyByIdAsync_ShouldReturnNull_WhenCompanyDoesNotExist()
{
    // Arrange
    await using var context =
        TestDbContextFactory.Create();

    var companyService =
        new CompanyService(context);

    // Act
    var result =
        await companyService.GetCompanyByIdAsync(
            id: 999);

    // Assert
    Assert.Null(result);
}

[Fact]
public async Task GetCompanyByIdAsync_ShouldReturnCompany_WhenCompanyExists()
{
    // Arrange
    await using var context =
        TestDbContextFactory.Create();

    var company =
        CreateCompany();

    context.Companies.Add(company);

    await context.SaveChangesAsync();

    var companyService =
        new CompanyService(context);

    // Act
    var result =
        await companyService.GetCompanyByIdAsync(
            company.Id);

    // Assert
    Assert.NotNull(result);

    Assert.Equal(
        company.Id,
        result.Id);

    Assert.Equal(
        company.Name,
        result.Name);

    Assert.Equal(
        company.Description,
        result.Description);

    Assert.Equal(
        company.WebsiteUrl,
        result.WebsiteUrl);

    Assert.Equal(
        company.LogoUrl,
        result.LogoUrl);

    Assert.Equal(
        company.Location,
        result.Location);

    Assert.Equal(
        company.CreatedAt,
        result.CreatedAt);

    Assert.Equal(
        0,
        result.TotalJobs);

    Assert.Empty(
        result.Jobs);
}

[Fact]
public async Task GetCompanyByIdAsync_ShouldReturnCompanyWithJobs()
{
    // Arrange
    await using var context =
        TestDbContextFactory.Create();

    var company =
        CreateCompany();

    context.Companies.Add(company);

    await context.SaveChangesAsync();

    var firstJob =
        CreateJob(
            company.Id,
            "Backend Developer");

    var secondJob =
        CreateJob(
            company.Id,
            "Frontend Developer");

    context.Jobs.AddRange(
        firstJob,
        secondJob);

    await context.SaveChangesAsync();

    var companyService =
        new CompanyService(context);

    // Act
    var result =
        await companyService.GetCompanyByIdAsync(
            company.Id);

    // Assert
    Assert.NotNull(result);

    Assert.Equal(
        2,
        result.TotalJobs);

    Assert.Equal(
        2,
        result.Jobs.Count);

    Assert.Contains(
        result.Jobs,
        job =>
            job.Id == firstJob.Id &&
            job.Title == firstJob.Title);

    Assert.Contains(
        result.Jobs,
        job =>
            job.Id == secondJob.Id &&
            job.Title == secondJob.Title);
}

[Fact]
public async Task CreateCompanyAsync_ShouldCreateCompany()
{
    // Arrange
    await using var context =
        TestDbContextFactory.Create();

    var companyService =
        new CompanyService(context);

    var request =
        CreateCompanyRequest();

    var beforeCreation =
        DateTime.UtcNow;

    // Act
    var result =
        await companyService.CreateCompanyAsync(
            request);

    var afterCreation =
        DateTime.UtcNow;

    // Assert
    var storedCompany =
        await context.Companies
            .SingleAsync();

    Assert.Equal(
        request.Name,
        storedCompany.Name);

    Assert.Equal(
        request.Description,
        storedCompany.Description);

    Assert.Equal(
        request.WebsiteUrl,
        storedCompany.WebsiteUrl);

    Assert.Equal(
        request.LogoUrl,
        storedCompany.LogoUrl);

    Assert.Equal(
        request.Location,
        storedCompany.Location);

    Assert.InRange(
        storedCompany.CreatedAt,
        beforeCreation,
        afterCreation);

    Assert.Equal(
        storedCompany.Id,
        result.Id);

    Assert.Equal(
        request.Name,
        result.Name);

    Assert.Equal(
        0,
        result.TotalJobs);
}

[Fact]
public async Task CreateCompanyAsync_ShouldConvertNullOptionalValuesToEmptyStringsInResponse()
{
    // Arrange
    await using var context =
        TestDbContextFactory.Create();

    var companyService =
        new CompanyService(context);

    var request =
        new CompanyRequest
        {
            Name = "Minimal Company",
            Description = null,
            WebsiteUrl = null,
            LogoUrl = null,
            Location = null
        };

    // Act
    var result =
        await companyService.CreateCompanyAsync(
            request);

    // Assert
    Assert.Equal(
        string.Empty,
        result.Description);

    Assert.Equal(
        string.Empty,
        result.WebsiteUrl);

    Assert.Equal(
        string.Empty,
        result.LogoUrl);

    Assert.Equal(
        string.Empty,
        result.Location);
}

[Fact]
public async Task UpdateCompanyAsync_ShouldReturnNull_WhenCompanyDoesNotExist()
{
    // Arrange
    await using var context =
        TestDbContextFactory.Create();

    var companyService =
        new CompanyService(context);

    var request =
        CreateCompanyRequest();

    // Act
    var result =
        await companyService.UpdateCompanyAsync(
            id: 999,
            request);

    // Assert
    Assert.Null(result);

    Assert.Empty(
        await context.Companies
            .ToListAsync());
}

[Fact]
public async Task UpdateCompanyAsync_ShouldUpdateCompany_WhenCompanyExists()
{
    // Arrange
    await using var context =
        TestDbContextFactory.Create();

    var company =
        CreateCompany();

    context.Companies.Add(company);

    await context.SaveChangesAsync();

    var originalCreatedAt =
        company.CreatedAt;

    var companyService =
        new CompanyService(context);

    var request =
        new CompanyRequest
        {
            Name = "Updated Company",
            Description = "Updated description",
            WebsiteUrl = "https://updated.example.com",
            LogoUrl = "https://updated.example.com/logo.png",
            Location = "Hamburg"
        };

    // Act
    var result =
        await companyService.UpdateCompanyAsync(
            company.Id,
            request);

    // Assert
    Assert.NotNull(result);

    var storedCompany =
        await context.Companies
            .SingleAsync();

    Assert.Equal(
        request.Name,
        storedCompany.Name);

    Assert.Equal(
        request.Description,
        storedCompany.Description);

    Assert.Equal(
        request.WebsiteUrl,
        storedCompany.WebsiteUrl);

    Assert.Equal(
        request.LogoUrl,
        storedCompany.LogoUrl);

    Assert.Equal(
        request.Location,
        storedCompany.Location);

    Assert.Equal(
        originalCreatedAt,
        storedCompany.CreatedAt);

    Assert.Equal(
        storedCompany.Id,
        result.Id);

    Assert.Equal(
        request.Name,
        result.Name);
}

[Fact]
public async Task DeleteCompanyAsync_ShouldReturnFalse_WhenCompanyDoesNotExist()
{
    // Arrange
    await using var context =
        TestDbContextFactory.Create();

    var companyService =
        new CompanyService(context);

    // Act
    var result =
        await companyService.DeleteCompanyAsync(
            id: 999);

    // Assert
    Assert.False(result);
}

[Fact]
public async Task DeleteCompanyAsync_ShouldDeleteCompany_WhenCompanyExists()
{
    // Arrange
    await using var context =
        TestDbContextFactory.Create();

    var company =
        CreateCompany();

    context.Companies.Add(company);

    await context.SaveChangesAsync();

    var companyService =
        new CompanyService(context);

    // Act
    var result =
        await companyService.DeleteCompanyAsync(
            company.Id);

    // Assert
    Assert.True(result);

    Assert.Empty(
        await context.Companies
            .ToListAsync());
}

[Fact]
public async Task UploadCompanyLogoAsync_ShouldReturnFailure_WhenCompanyDoesNotExist()
{
    // Arrange
    await using var context =
        TestDbContextFactory.Create();

    var companyService =
        new CompanyService(context);

    var file =
        CreateImageFormFile(
            "logo.png",
            "image/png");

    // Act
    var result =
        await companyService.UploadCompanyLogoAsync(
            id: 999,
            file);

    // Assert
    Assert.False(result.Success);

    Assert.Equal(
        "Company not found.",
        result.ErrorMessage);

    Assert.Null(
        result.LogoUrl);
}

[Fact]
public async Task UploadCompanyLogoAsync_ShouldReturnFailure_WhenFileIsNull()
{
    // Arrange
    await using var context =
        TestDbContextFactory.Create();

    var company =
        CreateCompany();

    context.Companies.Add(company);

    await context.SaveChangesAsync();

    var companyService =
        new CompanyService(context);

    // Act
    var result =
        await companyService.UploadCompanyLogoAsync(
            company.Id,
            null!);

    // Assert
    Assert.False(result.Success);

    Assert.Equal(
        "No file uploaded.",
        result.ErrorMessage);

    Assert.Null(
        result.LogoUrl);
}

[Fact]
public async Task UploadCompanyLogoAsync_ShouldReturnFailure_WhenFileIsEmpty()
{
    // Arrange
    await using var context =
        TestDbContextFactory.Create();

    var company =
        CreateCompany();

    context.Companies.Add(company);

    await context.SaveChangesAsync();

    var companyService =
        new CompanyService(context);

    var emptyFile =
        CreateEmptyFormFile(
            "logo.png",
            "image/png");

    // Act
    var result =
        await companyService.UploadCompanyLogoAsync(
            company.Id,
            emptyFile);

    // Assert
    Assert.False(result.Success);

    Assert.Equal(
        "No file uploaded.",
        result.ErrorMessage);

    Assert.Null(
        result.LogoUrl);
}

[Theory]
[InlineData("logo.gif", "image/gif")]
[InlineData("logo.pdf", "application/pdf")]
[InlineData("logo.svg", "image/svg+xml")]
[InlineData("logo.exe", "application/octet-stream")]
public async Task UploadCompanyLogoAsync_ShouldRejectUnsupportedFileExtensions(
    string fileName,
    string contentType)
{
    // Arrange
    await using var context =
        TestDbContextFactory.Create();

    var company =
        CreateCompany();

    context.Companies.Add(company);

    await context.SaveChangesAsync();

    var companyService =
        new CompanyService(context);

    var file =
        CreateImageFormFile(
            fileName,
            contentType);

    // Act
    var result =
        await companyService.UploadCompanyLogoAsync(
            company.Id,
            file);

    // Assert
    Assert.False(result.Success);

    Assert.Equal(
        "Only JPG, PNG and WEBP files are allowed.",
        result.ErrorMessage);

    Assert.Null(
        result.LogoUrl);

    var storedCompany =
        await context.Companies
            .SingleAsync();

    Assert.Equal(
        company.LogoUrl,
        storedCompany.LogoUrl);
}

[Theory]
[InlineData("logo.jpg", "image/jpeg")]
[InlineData("logo.jpeg", "image/jpeg")]
[InlineData("logo.png", "image/png")]
[InlineData("logo.webp", "image/webp")]
public async Task UploadCompanyLogoAsync_ShouldUploadAllowedImage(
    string fileName,
    string contentType)
{
    // Arrange
    await using var context =
        TestDbContextFactory.Create();

    var company =
        CreateCompany();

    company.LogoUrl =
        null;

    context.Companies.Add(company);

    await context.SaveChangesAsync();

    var companyService =
        new CompanyService(context);

    var file =
        CreateImageFormFile(
            fileName,
            contentType);

    string? physicalFilePath =
        null;

    try
    {
        // Act
        var result =
            await companyService.UploadCompanyLogoAsync(
                company.Id,
                file);

        // Assert
        Assert.True(result.Success);

        Assert.Null(
            result.ErrorMessage);

        Assert.NotNull(
            result.LogoUrl);

        Assert.StartsWith(
            $"/uploads/companies/company-{company.Id}-",
            result.LogoUrl);

        Assert.EndsWith(
            Path.GetExtension(fileName).ToLowerInvariant(),
            result.LogoUrl);

        var storedCompany =
            await context.Companies
                .SingleAsync();

        Assert.Equal(
            result.LogoUrl,
            storedCompany.LogoUrl);

        var relativeFilePath =
            result.LogoUrl!
                .TrimStart('/')
                .Replace(
                    '/',
                    Path.DirectorySeparatorChar);

        physicalFilePath =
            Path.Combine(
                Directory.GetCurrentDirectory(),
                "wwwroot",
                relativeFilePath);

        Assert.True(
            File.Exists(physicalFilePath));

        var storedBytes =
            await File.ReadAllBytesAsync(
                physicalFilePath);

        Assert.NotEmpty(
            storedBytes);
    }
    finally
    {
        if (!string.IsNullOrWhiteSpace(
                physicalFilePath) &&
            File.Exists(physicalFilePath))
        {
            File.Delete(physicalFilePath);
        }

        DeleteDirectoryWhenEmpty(
            Path.Combine(
                Directory.GetCurrentDirectory(),
                "wwwroot",
                "uploads",
                "companies"));

        DeleteDirectoryWhenEmpty(
            Path.Combine(
                Directory.GetCurrentDirectory(),
                "wwwroot",
                "uploads"));

        DeleteDirectoryWhenEmpty(
            Path.Combine(
                Directory.GetCurrentDirectory(),
                "wwwroot"));
    }
}

private static Company CreateCompany(
    string name = "SkillJob Test Company",
    DateTime? createdAt = null)
{
    return new Company
    {
        Name = name,
        Description = "Test company description",
        WebsiteUrl = "https://example.com",
        LogoUrl = "https://example.com/logo.png",
        Location = "Berlin",
        CreatedAt = createdAt ?? DateTime.UtcNow
    };
}

private static CompanyRequest CreateCompanyRequest()
{
    return new CompanyRequest
    {
        Name = "New Test Company",
        Description = "New company description",
        WebsiteUrl = "https://new-company.example.com",
        LogoUrl = "https://new-company.example.com/logo.png",
        Location = "Munich"
    };
}

private static Job CreateJob(
    int companyId,
    string title)
{
    return new Job
    {
        Title = title,
        Description = "Test job description",
        Location = "Berlin",
        Salary = "60.000 - 80.000 EUR",
        CompanyId = companyId,
        CreatedAt = DateTime.UtcNow
    };
}

private static IFormFile CreateImageFormFile(
    string fileName,
    string contentType)
{
    var content =
        Encoding.UTF8.GetBytes(
            "fake image content");

    var stream =
        new MemoryStream(content);

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
            contentType
    };
}

private static IFormFile CreateEmptyFormFile(
    string fileName,
    string contentType)
{
    var stream =
        new MemoryStream();

    return new FormFile(
        stream,
        0,
        0,
        "file",
        fileName)
    {
        Headers =
            new HeaderDictionary(),
        ContentType =
            contentType
    };
}

private static void DeleteDirectoryWhenEmpty(
    string directoryPath)
{
    if (!Directory.Exists(directoryPath))
        return;

    if (Directory
        .EnumerateFileSystemEntries(directoryPath)
        .Any())
    {
        return;
    }

    Directory.Delete(directoryPath);
}


}
