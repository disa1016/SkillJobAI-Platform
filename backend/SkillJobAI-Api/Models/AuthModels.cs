namespace SkillJobAI.Api.Models;

public class RegisterRequest
{
    public string FullName { get; set; } = "";
    public string Email { get; set; } = "";
    public string Password { get; set; } = "";
    public string Role { get; set; } = "Student";
}

public class LoginRequest
{
    public string Email { get; set; } = "";
    public string Password { get; set; } = "";
}

public class ForgotPasswordRequest
{
    public string Email { get; set; } = "";
    public string NewPassword { get; set; } = "";
}

