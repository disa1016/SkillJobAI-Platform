using SkillJobAI.Api.Services;

namespace SkillJobAI.Api.Tests.Services;

public class PasswordServiceTests
{
    private readonly PasswordService _passwordService = new();

    [Fact]
    public void HashPassword_ShouldReturnHashDifferentFromPlainText()
    {
        // Arrange
        const string password = "SecurePassword123!";

        // Act
        var hash = _passwordService.HashPassword(password);

        // Assert
        Assert.False(string.IsNullOrWhiteSpace(hash));
        Assert.NotEqual(password, hash);
    }

    [Fact]
    public void VerifyPassword_ShouldReturnTrue_WhenPasswordIsCorrect()
    {
        // Arrange
        const string password = "SecurePassword123!";
        var hash = _passwordService.HashPassword(password);

        // Act
        var result = _passwordService.VerifyPassword(
            password,
            hash);

        // Assert
        Assert.True(result);
    }

    [Fact]
    public void VerifyPassword_ShouldReturnFalse_WhenPasswordIsIncorrect()
    {
        // Arrange
        const string correctPassword = "SecurePassword123!";
        const string incorrectPassword = "WrongPassword123!";

        var hash = _passwordService.HashPassword(
            correctPassword);

        // Act
        var result = _passwordService.VerifyPassword(
            incorrectPassword,
            hash);

        // Assert
        Assert.False(result);
    }
}