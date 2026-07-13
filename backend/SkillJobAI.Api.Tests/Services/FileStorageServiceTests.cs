using Microsoft.AspNetCore.Http;
using SkillJobAI.Api.Services;
using System.Text;

namespace SkillJobAI.Api.Tests.Services;

public class FileStorageServiceTests
{
[Fact]
public async Task SavePdfFileAsync_ShouldReturnNull_WhenFileIsNull()
{
// Arrange
var service =
new FileStorageService();

    // Act
    var result =
        await service.SavePdfFileAsync(
            file: null,
            folderName: "cv",
            userId: 1);

    // Assert
    Assert.Null(result);
}

[Fact]
public async Task SavePdfFileAsync_ShouldReturnNull_WhenFileIsEmpty()
{
    // Arrange
    var service =
        new FileStorageService();

    using var stream =
        new MemoryStream();

    var file =
        new FormFile(
            stream,
            0,
            0,
            "file",
            "empty.pdf")
        {
            Headers =
                new HeaderDictionary(),
            ContentType =
                "application/pdf"
        };

    // Act
    var result =
        await service.SavePdfFileAsync(
            file,
            folderName: "cv",
            userId: 1);

    // Assert
    Assert.Null(result);
}

[Theory]
[InlineData("document.txt")]
[InlineData("document.jpg")]
[InlineData("document.png")]
[InlineData("document.docx")]
[InlineData("document.exe")]
public async Task SavePdfFileAsync_ShouldThrow_WhenFileExtensionIsNotPdf(
    string fileName)
{
    // Arrange
    var service =
        new FileStorageService();

    using var file =
        CreateFormFile(
            fileName,
            "test content");

    // Act
    var exception =
        await Assert.ThrowsAsync<InvalidOperationException>(
            () =>
                service.SavePdfFileAsync(
                    file,
                    folderName: "cv",
                    userId: 1));

    // Assert
    Assert.Equal(
        "Only PDF files are allowed.",
        exception.Message);
}

[Fact]
public async Task SavePdfFileAsync_ShouldAcceptUppercasePdfExtension()
{
    // Arrange
    var service =
        new FileStorageService();

    using var file =
        CreateFormFile(
            "candidate-cv.PDF",
            "%PDF-1.4 test content");

    string? physicalFilePath =
        null;

    try
    {
        // Act
        var result =
            await service.SavePdfFileAsync(
                file,
                folderName: "cv",
                userId: 10);

        // Assert
        Assert.NotNull(result);

        physicalFilePath =
            ConvertRelativePathToPhysicalPath(
                result);

        Assert.True(
            File.Exists(physicalFilePath));
    }
    finally
    {
        DeleteFileAndEmptyDirectories(
            physicalFilePath);
    }
}

[Fact]
public async Task SavePdfFileAsync_ShouldThrow_WhenFileIsLargerThanFiveMegabytes()
{
    // Arrange
    var service =
        new FileStorageService();

    const int maximumFileSize =
        5 * 1024 * 1024;

    var content =
        new byte[maximumFileSize + 1];

    using var stream =
        new MemoryStream(content);

    var file =
        new FormFile(
            stream,
            0,
            stream.Length,
            "file",
            "large-file.pdf")
        {
            Headers =
                new HeaderDictionary(),
            ContentType =
                "application/pdf"
        };

    // Act
    var exception =
        await Assert.ThrowsAsync<InvalidOperationException>(
            () =>
                service.SavePdfFileAsync(
                    file,
                    folderName: "cv",
                    userId: 1));

    // Assert
    Assert.Equal(
        "PDF file must be smaller than 5MB.",
        exception.Message);
}

[Theory]
[InlineData("profile")]
[InlineData("documents")]
[InlineData("certificate")]
[InlineData("../cv")]
[InlineData("../../private_uploads")]
[InlineData("")]
[InlineData(" ")]
public async Task SavePdfFileAsync_ShouldThrow_WhenFolderIsNotAllowed(
    string folderName)
{
    // Arrange
    var service =
        new FileStorageService();

    using var file =
        CreateFormFile(
            "document.pdf",
            "%PDF-1.4 test content");

    // Act
    var exception =
        await Assert.ThrowsAsync<InvalidOperationException>(
            () =>
                service.SavePdfFileAsync(
                    file,
                    folderName,
                    userId: 1));

    // Assert
    Assert.Equal(
        "Invalid upload folder.",
        exception.Message);
}

[Theory]
[InlineData("cv")]
[InlineData("certificates")]
[InlineData("portfolio")]
public async Task SavePdfFileAsync_ShouldSavePdfInAllowedFolder(
    string folderName)
{
    // Arrange
    var service =
        new FileStorageService();

    const int userId =
        42;

    const string fileContent =
        "%PDF-1.4 SkillJobAI test document";

    using var file =
        CreateFormFile(
            "document.pdf",
            fileContent);

    string? physicalFilePath =
        null;

    try
    {
        // Act
        var result =
            await service.SavePdfFileAsync(
                file,
                folderName,
                userId);

        // Assert
        Assert.NotNull(result);

        Assert.StartsWith(
            $"private_uploads/applications/{folderName}/",
            result);

        Assert.Contains(
            $"{folderName}-user-{userId}-",
            result);

        Assert.EndsWith(
            ".pdf",
            result);

        Assert.DoesNotContain(
            '\\',
            result);

        physicalFilePath =
            ConvertRelativePathToPhysicalPath(
                result);

        Assert.True(
            File.Exists(physicalFilePath));

        var storedContent =
            await File.ReadAllTextAsync(
                physicalFilePath);

        Assert.Equal(
            fileContent,
            storedContent);
    }
    finally
    {
        DeleteFileAndEmptyDirectories(
            physicalFilePath);
    }
}

[Fact]
public async Task SavePdfFileAsync_ShouldNormalizeFolderName()
{
    // Arrange
    var service =
        new FileStorageService();

    using var file =
        CreateFormFile(
            "document.pdf",
            "%PDF-1.4 test content");

    string? physicalFilePath =
        null;

    try
    {
        // Act
        var result =
            await service.SavePdfFileAsync(
                file,
                folderName: "  CV  ",
                userId: 15);

        // Assert
        Assert.NotNull(result);

        Assert.StartsWith(
            "private_uploads/applications/cv/",
            result);

        Assert.Contains(
            "cv-user-15-",
            result);

        physicalFilePath =
            ConvertRelativePathToPhysicalPath(
                result);

        Assert.True(
            File.Exists(physicalFilePath));
    }
    finally
    {
        DeleteFileAndEmptyDirectories(
            physicalFilePath);
    }
}

[Fact]
public async Task SavePdfFileAsync_ShouldGenerateUniqueFileNames()
{
    // Arrange
    var service =
        new FileStorageService();

    using var firstFile =
        CreateFormFile(
            "cv.pdf",
            "%PDF-1.4 first document");

    using var secondFile =
        CreateFormFile(
            "cv.pdf",
            "%PDF-1.4 second document");

    string? firstPhysicalFilePath =
        null;

    string? secondPhysicalFilePath =
        null;

    try
    {
        // Act
        var firstResult =
            await service.SavePdfFileAsync(
                firstFile,
                folderName: "cv",
                userId: 5);

        var secondResult =
            await service.SavePdfFileAsync(
                secondFile,
                folderName: "cv",
                userId: 5);

        // Assert
        Assert.NotNull(firstResult);
        Assert.NotNull(secondResult);

        Assert.NotEqual(
            firstResult,
            secondResult);

        firstPhysicalFilePath =
            ConvertRelativePathToPhysicalPath(
                firstResult);

        secondPhysicalFilePath =
            ConvertRelativePathToPhysicalPath(
                secondResult);

        Assert.True(
            File.Exists(firstPhysicalFilePath));

        Assert.True(
            File.Exists(secondPhysicalFilePath));
    }
    finally
    {
        DeleteFileAndEmptyDirectories(
            firstPhysicalFilePath);

        DeleteFileAndEmptyDirectories(
            secondPhysicalFilePath);
    }
}

[Fact]
public async Task SavePdfFileAsync_ShouldCreateUploadDirectory_WhenDirectoryDoesNotExist()
{
    // Arrange
    var service =
        new FileStorageService();

    var uploadsDirectory =
        Path.Combine(
            Directory.GetCurrentDirectory(),
            "private_uploads",
            "applications",
            "portfolio");

    DeleteDirectoryWhenEmpty(
        uploadsDirectory);

    using var file =
        CreateFormFile(
            "portfolio.pdf",
            "%PDF-1.4 portfolio content");

    string? physicalFilePath =
        null;

    try
    {
        // Act
        var result =
            await service.SavePdfFileAsync(
                file,
                folderName: "portfolio",
                userId: 77);

        // Assert
        Assert.NotNull(result);

        Assert.True(
            Directory.Exists(
                uploadsDirectory));

        physicalFilePath =
            ConvertRelativePathToPhysicalPath(
                result);

        Assert.True(
            File.Exists(
                physicalFilePath));
    }
    finally
    {
        DeleteFileAndEmptyDirectories(
            physicalFilePath);
    }
}

[Fact]
public async Task SavePdfFileAsync_ShouldAcceptFileAtExactlyFiveMegabytes()
{
    // Arrange
    var service =
        new FileStorageService();

    const int maximumFileSize =
        5 * 1024 * 1024;

    var content =
        new byte[maximumFileSize];

    content[0] =
        (byte)'%';

    content[1] =
        (byte)'P';

    content[2] =
        (byte)'D';

    content[3] =
        (byte)'F';

    using var stream =
        new MemoryStream(content);

    var file =
        new FormFile(
            stream,
            0,
            stream.Length,
            "file",
            "maximum-size.pdf")
        {
            Headers =
                new HeaderDictionary(),
            ContentType =
                "application/pdf"
        };

    string? physicalFilePath =
        null;

    try
    {
        // Act
        var result =
            await service.SavePdfFileAsync(
                file,
                folderName: "cv",
                userId: 99);

        // Assert
        Assert.NotNull(result);

        physicalFilePath =
            ConvertRelativePathToPhysicalPath(
                result);

        Assert.True(
            File.Exists(physicalFilePath));

        var fileInfo =
            new FileInfo(
                physicalFilePath);

        Assert.Equal(
            maximumFileSize,
            fileInfo.Length);
    }
    finally
    {
        DeleteFileAndEmptyDirectories(
            physicalFilePath);
    }
}

private static TestFormFile CreateFormFile(
    string fileName,
    string content)
{
    var bytes =
        Encoding.UTF8.GetBytes(
            content);

    var stream =
        new MemoryStream(
            bytes);

    var formFile =
        new FormFile(
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

    return new TestFormFile(
        stream,
        formFile);
}

private static string ConvertRelativePathToPhysicalPath(
    string relativePath)
{
    var normalizedRelativePath =
        relativePath.Replace(
            '/',
            Path.DirectorySeparatorChar);

    return Path.GetFullPath(
        Path.Combine(
            Directory.GetCurrentDirectory(),
            normalizedRelativePath));
}

private static void DeleteFileAndEmptyDirectories(
    string? physicalFilePath)
{
    if (string.IsNullOrWhiteSpace(
            physicalFilePath))
    {
        return;
    }

    if (File.Exists(
            physicalFilePath))
    {
        File.Delete(
            physicalFilePath);
    }

    var folder =
        Path.GetDirectoryName(
            physicalFilePath);

    DeleteDirectoryWhenEmpty(
        folder);

    if (folder != null)
    {
        DeleteDirectoryWhenEmpty(
            Directory.GetParent(folder)
                ?.FullName);

        var applicationsFolder =
            Directory.GetParent(folder)
                ?.FullName;

        if (applicationsFolder != null)
        {
            DeleteDirectoryWhenEmpty(
                Directory.GetParent(
                    applicationsFolder)
                    ?.FullName);
        }
    }
}

private static void DeleteDirectoryWhenEmpty(
    string? directoryPath)
{
    if (string.IsNullOrWhiteSpace(
            directoryPath))
    {
        return;
    }

    if (!Directory.Exists(
            directoryPath))
    {
        return;
    }

    if (Directory
        .EnumerateFileSystemEntries(
            directoryPath)
        .Any())
    {
        return;
    }

    Directory.Delete(
        directoryPath);
}

private sealed class TestFormFile : IFormFile, IDisposable
{
    private readonly MemoryStream _stream;
    private readonly IFormFile _formFile;

    public TestFormFile(
        MemoryStream stream,
        IFormFile formFile)
    {
        _stream = stream;
        _formFile = formFile;
    }

    public string ContentType =>
        _formFile.ContentType;

    public string ContentDisposition =>
        _formFile.ContentDisposition;

    public IHeaderDictionary Headers =>
        _formFile.Headers;

    public long Length =>
        _formFile.Length;

    public string Name =>
        _formFile.Name;

    public string FileName =>
        _formFile.FileName;

    public void CopyTo(
        Stream target)
    {
        _formFile.CopyTo(
            target);
    }

    public Task CopyToAsync(
        Stream target,
        CancellationToken cancellationToken = default)
    {
        return _formFile.CopyToAsync(
            target,
            cancellationToken);
    }

    public Stream OpenReadStream()
    {
        return _formFile.OpenReadStream();
    }

    public void Dispose()
    {
        _stream.Dispose();
    }
}

}
