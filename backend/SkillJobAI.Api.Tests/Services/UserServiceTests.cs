using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Moq;
using SkillJobAI.Api.Data;
using SkillJobAI.Api.Entities;
using SkillJobAI.Api.Models;
using SkillJobAI.Api.Services;
using SkillJobAI.Api.Tests.Helpers;
using Microsoft.Extensions.Logging;



namespace SkillJobAI.Api.Tests.Services;

public class UserServiceTests
{
    [Fact]
    public async Task GetProfileAsync_ShouldReturnNull_WhenUserDoesNotExist()
    {
        // Arrange
        await using var context =
            TestDbContextFactory.Create();

        var userService =
            CreateUserService(context);

        // Act
        var result =
            await userService.GetProfileAsync(
                userId: 999);

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public async Task GetProfileAsync_ShouldReturnUser_WhenUserExists()
    {
        // Arrange
        await using var context =
            TestDbContextFactory.Create();

        var createdAt =
            DateTime.UtcNow.AddDays(-1);

        var user =
            new AppUser
            {
                FullName = "Test Candidate",
                Email = "candidate@test.com",
                PasswordHash = "test-hash",
                Role = "Candidate",
                CvUrl = "/uploads/cv/test-cv.pdf",
                CreatedAt = createdAt
            };

        context.Users.Add(user);
        await context.SaveChangesAsync();

        var userService =
            CreateUserService(context);

        // Act
        var result =
            await userService.GetProfileAsync(
                user.Id);

        // Assert
        Assert.NotNull(result);

        Assert.Equal(
            user.Id,
            result.Id);

        Assert.Equal(
            user.FullName,
            result.FullName);

        Assert.Equal(
            user.Email,
            result.Email);

        Assert.Equal(
            user.Role,
            result.Role);

        Assert.Equal(
            "/api/users/cv",
            result.CvUrl);

        Assert.Equal(
            createdAt,
            result.CreatedAt);
    }

    [Fact]
    public async Task GetProfileAsync_ShouldReturnEmptyCvUrl_WhenCvUrlIsNull()
    {
        // Arrange
        await using var context =
            TestDbContextFactory.Create();

        var user =
            CreateUser();

        user.CvUrl = null;

        context.Users.Add(user);
        await context.SaveChangesAsync();

        var userService =
            CreateUserService(context);

        // Act
        var result =
            await userService.GetProfileAsync(
                user.Id);

        // Assert
        Assert.NotNull(result);

        Assert.Equal(
            string.Empty,
            result.CvUrl);
    }

    [Fact]
    public async Task UploadCvAsync_ShouldReturnFailure_WhenUserDoesNotExist()
    {
        // Arrange
        await using var context =
            TestDbContextFactory.Create();

        var userService =
            CreateUserService(context);

        var testFile =
            CreatePdfFormFile();

        using var stream =
            testFile.Stream;

        // Act
        var result =
            await userService.UploadCvAsync(
                userId: 999,
                testFile.File);

        // Assert
        Assert.False(result.Success);

        Assert.Equal(
            "User not found.",
            result.ErrorMessage);

        Assert.Null(result.CvUrl);
    }

    [Fact]
    public async Task UploadCvAsync_ShouldReturnFailure_WhenFileIsNull()
    {
        // Arrange
        await using var context =
            TestDbContextFactory.Create();

        var user =
            CreateUser();

        context.Users.Add(user);
        await context.SaveChangesAsync();

        var userService =
            CreateUserService(context);

        // Act
        var result =
            await userService.UploadCvAsync(
                user.Id,
                null!);

        // Assert
        Assert.False(result.Success);

        Assert.Equal(
            "No file uploaded.",
            result.ErrorMessage);

        Assert.Null(result.CvUrl);
    }

    [Fact]
    public async Task UploadCvAsync_ShouldReturnFailure_WhenFileIsEmpty()
    {
        // Arrange
        await using var context =
            TestDbContextFactory.Create();

        var user =
            CreateUser();

        context.Users.Add(user);
        await context.SaveChangesAsync();

        var userService =
            CreateUserService(context);

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
            await userService.UploadCvAsync(
                user.Id,
                file);

        // Assert
        Assert.False(result.Success);

        Assert.Equal(
            "No file uploaded.",
            result.ErrorMessage);

        Assert.Null(result.CvUrl);
    }

    [Fact]
    public async Task UploadCvAsync_ShouldReturnFailure_WhenFileIsNotPdf()
    {
        // Arrange
        await using var context =
            TestDbContextFactory.Create();

        var user =
            CreateUser();

        context.Users.Add(user);
        await context.SaveChangesAsync();

        var userService =
            CreateUserService(context);

        using var stream =
            new MemoryStream(
                new byte[]
                {
                    1,
                    2,
                    3
                });

        var file =
            new FormFile(
                stream,
                0,
                stream.Length,
                "file",
                "cv.txt");

        // Act
        var result =
            await userService.UploadCvAsync(
                user.Id,
                file);

        // Assert
        Assert.False(result.Success);

        Assert.Equal(
            "Only PDF files are allowed.",
            result.ErrorMessage);

        Assert.Null(result.CvUrl);
    }

    [Fact]
    public async Task UploadCvAsync_ShouldReturnFailure_WhenFileIsTooLarge()
    {
        // Arrange
        await using var context =
            TestDbContextFactory.Create();

        var user =
            CreateUser();

        context.Users.Add(user);
        await context.SaveChangesAsync();

        var userService =
            CreateUserService(context);

        var fileMock =
            new Mock<IFormFile>();

        fileMock
            .SetupGet(file => file.FileName)
            .Returns("large-cv.pdf");

        fileMock
            .SetupGet(file => file.Length)
            .Returns(
                5L * 1024 * 1024 + 1);

        // Act
        var result =
            await userService.UploadCvAsync(
                user.Id,
                fileMock.Object);

        // Assert
        Assert.False(result.Success);

        Assert.Equal(
            "PDF file must be smaller than 5MB.",
            result.ErrorMessage);

        Assert.Null(result.CvUrl);

        fileMock.Verify(
            file => file.CopyToAsync(
                It.IsAny<Stream>(),
                It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task UploadCvAsync_ShouldSaveFileAndUpdateUser_WhenFileIsValid()
    {
        // Arrange
        await using var context =
            TestDbContextFactory.Create();

        var user =
            CreateUser();

        context.Users.Add(user);
        await context.SaveChangesAsync();

        var userService =
            CreateUserService(context);

        var testFile =
            CreatePdfFormFile();

        using var stream =
            testFile.Stream;

        string? physicalFilePath = null;

        try
        {
            // Act
            var result =
                await userService.UploadCvAsync(
                    user.Id,
                    testFile.File);

            // Assert
            Assert.True(result.Success);
            Assert.Null(result.ErrorMessage);

            Assert.Equal(
                "/api/users/cv",
                result.CvUrl);

            context.ChangeTracker.Clear();

            var storedUser =
                await context.Users
                    .SingleAsync(
                        existingUser =>
                            existingUser.Id ==
                            user.Id);

            Assert.False(
                string.IsNullOrWhiteSpace(
                    storedUser.CvUrl));

            Assert.StartsWith(
                "profile-cv/cv-user-" +
                $"{user.Id}-",
                storedUser.CvUrl);

            Assert.EndsWith(
                ".pdf",
                storedUser.CvUrl);

            physicalFilePath =
                ConvertPrivateCvPathToPhysicalPath(
                    storedUser.CvUrl!);

            Assert.True(
                File.Exists(
                    physicalFilePath));

            var storedBytes =
                await File.ReadAllBytesAsync(
                    physicalFilePath);

            Assert.Equal(
                GetValidPdfBytes(),
                storedBytes);
        }
        finally
        {
            DeleteFileIfExists(
                physicalFilePath);
        }
    }

    [Fact]
    public async Task DeleteCvAsync_ShouldReturnFalse_WhenUserDoesNotExist()
    {
        // Arrange
        await using var context =
            TestDbContextFactory.Create();

        var userService =
            CreateUserService(context);

        // Act
        var result =
            await userService.DeleteCvAsync(
                userId: 999);

        // Assert
        Assert.False(result);
    }

    [Fact]
    public async Task DeleteCvAsync_ShouldReturnTrue_WhenUserHasNoCv()
    {
        // Arrange
        await using var context =
            TestDbContextFactory.Create();

        var user =
            CreateUser();

        user.CvUrl = null;

        context.Users.Add(user);
        await context.SaveChangesAsync();

        var userService =
            CreateUserService(context);

        // Act
        var result =
            await userService.DeleteCvAsync(
                user.Id);

        // Assert
        Assert.True(result);

        context.ChangeTracker.Clear();

        var storedUser =
            await context.Users
                .SingleAsync(
                    existingUser =>
                        existingUser.Id ==
                        user.Id);

        Assert.Null(storedUser.CvUrl);
    }

    [Fact]
    public async Task DeleteCvAsync_ShouldDeletePhysicalFileAndClearCvUrl()
    {
        // Arrange
        await using var context =
            TestDbContextFactory.Create();

        var user =
            CreateUser();

        var relativeUrl =
            $"/uploads/cv/delete-test-{Guid.NewGuid():N}.pdf";

        var physicalFilePath =
            ConvertUrlToPhysicalPath(
                relativeUrl);

        var directory =
            Path.GetDirectoryName(
                physicalFilePath)!;

        Directory.CreateDirectory(
            directory);

        await File.WriteAllBytesAsync(
            physicalFilePath,
            new byte[]
            {
                1,
                2,
                3
            });

        user.CvUrl =
            relativeUrl;

        context.Users.Add(user);
        await context.SaveChangesAsync();

        var userService =
            CreateUserService(context);

        try
        {
            Assert.True(
                File.Exists(
                    physicalFilePath));

            // Act
            var result =
                await userService.DeleteCvAsync(
                    user.Id);

            // Assert
            Assert.True(result);

            Assert.False(
                File.Exists(
                    physicalFilePath));

            context.ChangeTracker.Clear();

            var storedUser =
                await context.Users
                    .SingleAsync(
                        existingUser =>
                            existingUser.Id ==
                            user.Id);

            Assert.Null(
                storedUser.CvUrl);
        }
        finally
        {
            DeleteFileIfExists(
                physicalFilePath);
        }
    }

    [Fact]
    public async Task DeleteCvAsync_ShouldClearCvUrl_WhenPhysicalFileDoesNotExist()
    {
        // Arrange
        await using var context =
            TestDbContextFactory.Create();

        var user =
            CreateUser();

        user.CvUrl =
            $"/uploads/cv/missing-{Guid.NewGuid():N}.pdf";

        context.Users.Add(user);
        await context.SaveChangesAsync();

        var userService =
            CreateUserService(context);

        // Act
        var result =
            await userService.DeleteCvAsync(
                user.Id);

        // Assert
        Assert.True(result);

        context.ChangeTracker.Clear();

        var storedUser =
            await context.Users
                .SingleAsync(
                    existingUser =>
                        existingUser.Id ==
                        user.Id);

        Assert.Null(
            storedUser.CvUrl);
    }

    [Fact]
    public async Task UpdateUserRoleAsync_ShouldReturnNull_WhenUserDoesNotExist()
    {
        // Arrange
        await using var context =
            TestDbContextFactory.Create();

        var userService =
            CreateUserService(context);

        var request =
            new UpdateUserRoleRequest
            {
                Role = "Admin"
            };

        // Act
        var result =
            await userService.UpdateUserRoleAsync(
                id: 999,
                request);

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public async Task UpdateUserRoleAsync_ShouldUpdateRole_WhenUserExists()
    {
        // Arrange
        await using var context =
            TestDbContextFactory.Create();

        var user =
            CreateUser();

        context.Users.Add(user);
        await context.SaveChangesAsync();

        var userService =
            CreateUserService(context);

        var request =
            new UpdateUserRoleRequest
            {
                Role = "Recruiter"
            };

        // Act
        var result =
            await userService.UpdateUserRoleAsync(
                user.Id,
                request);

        // Assert
        Assert.NotNull(result);

        Assert.Equal(
            user.Id,
            result.Id);

        Assert.Equal(
            user.FullName,
            result.FullName);

        Assert.Equal(
            user.Email,
            result.Email);

        Assert.Equal(
            "Recruiter",
            result.Role);

        Assert.Equal(
            string.Empty,
            result.CvUrl);

        context.ChangeTracker.Clear();

        var storedUser =
            await context.Users
                .SingleAsync(
                    existingUser =>
                        existingUser.Id ==
                        user.Id);

        Assert.Equal(
            "Recruiter",
            storedUser.Role);
    }

  private static UserService CreateUserService(
    AppDbContext context)
{
    var currentDirectory =
        Directory.GetCurrentDirectory();

    var webRootPath =
        Path.Combine(
            currentDirectory,
            "wwwroot");

    Directory.CreateDirectory(
        webRootPath);

    var environment =
        new Mock<IWebHostEnvironment>();

    environment
        .SetupGet(value => value.ContentRootPath)
        .Returns(currentDirectory);

    environment
        .SetupGet(value => value.WebRootPath)
        .Returns(webRootPath);

    environment
        .SetupGet(value => value.EnvironmentName)
        .Returns("Testing");

    var passwordService =
        new PasswordService();

    var logger =
        Mock.Of<ILogger<UserService>>();

    return new UserService(
        context,
        environment.Object,
        passwordService,
        logger);
}

    private static AppUser CreateUser()
    {
        return new AppUser
        {
            FullName = "Test Candidate",

            Email =
                $"candidate-{Guid.NewGuid():N}@test.com",

            PasswordHash =
                "test-password-hash",

            Role =
                "Candidate",

            CreatedAt =
                DateTime.UtcNow
        };
    }

    private static (
        FormFile File,
        MemoryStream Stream)
        CreatePdfFormFile()
    {
        var bytes =
            GetValidPdfBytes();

        var stream =
            new MemoryStream(bytes);

        var file =
            new FormFile(
                stream,
                0,
                stream.Length,
                "file",
                "candidate-cv.PDF")
            {
                Headers =
                    new HeaderDictionary(),

                ContentType =
                    "application/pdf"
            };

        return (
            file,
            stream);
    }

    private static byte[] GetValidPdfBytes()
    {
        return
        [
            (byte)'%',
            (byte)'P',
            (byte)'D',
            (byte)'F',
            (byte)'-',
            (byte)'1',
            (byte)'.',
            (byte)'4',
            (byte)'\n',
            (byte)'%',
            (byte)'%',
            (byte)'E',
            (byte)'O',
            (byte)'F'
        ];
    }

    private static string ConvertUrlToPhysicalPath(
        string cvUrl)
    {
        var relativePath =
            cvUrl
                .TrimStart('/')
                .Replace(
                    '/',
                    Path.DirectorySeparatorChar);

        return Path.Combine(
            Directory.GetCurrentDirectory(),
            "wwwroot",
            relativePath);
    }

    private static string ConvertPrivateCvPathToPhysicalPath(
        string storedPath)
    {
        var relativePath =
            storedPath
                .TrimStart('/', '\\')
                .Replace(
                    '/',
                    Path.DirectorySeparatorChar)
                .Replace(
                    '\\',
                    Path.DirectorySeparatorChar);

        return Path.Combine(
            Directory.GetCurrentDirectory(),
            "private_uploads",
            relativePath);
    }

    private static void DeleteFileIfExists(
        string? filePath)
    {
        if (string.IsNullOrWhiteSpace(
                filePath))
        {
            return;
        }

        if (File.Exists(filePath))
        {
            File.Delete(filePath);
        }
    }
}