using System.Text;
using System.Threading.RateLimiting;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using Serilog;
using SkillJobAI.Api.Data;
using SkillJobAI.Api.Middleware;
using SkillJobAI.Api.Models;
using SkillJobAI.Api.Services;

var builder = WebApplication.CreateBuilder(args);

// ----------------------------
// Serilog konfigurieren
// ----------------------------
Log.Logger = new LoggerConfiguration()
    .ReadFrom.Configuration(builder.Configuration)
    .Enrich.FromLogContext()
    .WriteTo.Console()
    .WriteTo.File(
        "Logs/log-.txt",
        rollingInterval: RollingInterval.Day,
        retainedFileCountLimit: 30,
        shared: true)
    .CreateLogger();

builder.Host.UseSerilog();

// ----------------------------
// Controller aktivieren
// ----------------------------
builder.Services.AddControllers();

builder.Services.Configure<FormOptions>(options =>
{
    options.MultipartBodyLengthLimit =
        10 * 1024 * 1024;
});

// ----------------------------
// Connection String
// ----------------------------
var defaultConnection =
    builder.Configuration.GetConnectionString(
        "DefaultConnection");

if (builder.Environment.IsEnvironment("Testing"))
{
    // Integrationstests verwenden eine InMemory-Datenbank.
    builder.Services.AddHealthChecks();
}
else
{
    if (string.IsNullOrWhiteSpace(defaultConnection))
    {
        throw new InvalidOperationException(
            "Die Connection String " +
            "'ConnectionStrings:DefaultConnection' fehlt.");
    }

    builder.Services
        .AddHealthChecks()
        .AddNpgSql(
            defaultConnection,
            name: "postgres");
}

// ----------------------------
// Services registrieren
// ----------------------------
builder.Services.AddScoped<
    IAuthService,
    AuthService>();

builder.Services.AddScoped<
    IJobService,
    JobService>();

builder.Services.AddScoped<
    ICompanyService,
    CompanyService>();

builder.Services.AddScoped<
    ICourseService,
    CourseService>();

builder.Services.AddScoped<
    ILessonService,
    LessonService>();

builder.Services.AddScoped<
    IApplicationService,
    ApplicationService>();

builder.Services.AddScoped<
    IUserService,
    UserService>();

builder.Services.AddScoped<
    IEnrollmentService,
    EnrollmentService>();

builder.Services.AddScoped<
    ICertificateService,
    CertificateService>();

builder.Services.AddScoped<
    ISkillGapService,
    SkillGapService>();

builder.Services.AddScoped<
    IProgressService,
    ProgressService>();

builder.Services.AddScoped<
    IUserSkillService,
    UserSkillService>();

builder.Services.AddScoped<
    IJobSkillService,
    JobSkillService>();

builder.Services.AddScoped<
    ICompanyMemberService,
    CompanyMemberService>();

builder.Services.AddScoped<
    ICandidateDashboardService,
    CandidateDashboardService>();

builder.Services.AddScoped<
    ISkillService,
    SkillService>();

builder.Services.AddScoped<
    ICourseSkillService,
    CourseSkillService>();

builder.Services.AddScoped<
    IAdminService,
    AdminService>();

builder.Services.AddScoped<
    IRecruiterService,
    RecruiterService>();

builder.Services.AddScoped<
    IRecruiterCandidateService,
    RecruiterCandidateService>();

builder.Services.AddScoped<
    ICareerRoadmapService,
    CareerRoadmapService>();

builder.Services.AddScoped<
    IAiService,
    AiService>();

builder.Services.AddScoped<
    IEmailService,
    EmailService>();

builder.Services.AddScoped<
    IFileStorageService,
    FileStorageService>();

builder.Services.AddScoped<
    IApplicationMatchingService,
    ApplicationMatchingService>();

// ----------------------------
// Rate Limiting
// ----------------------------
if (builder.Environment.IsEnvironment("Testing"))
{
    builder.Services.AddRateLimiter(options =>
    {
        options.AddPolicy(
            "auth",
            _ => RateLimitPartition
                .GetNoLimiter("auth-tests"));

        options.AddPolicy(
            "ai",
            _ => RateLimitPartition
                .GetNoLimiter("ai-tests"));
    });
}
else
{
    builder.Services.AddRateLimiter(options =>
    {
        options.RejectionStatusCode =
            StatusCodes.Status429TooManyRequests;

        options.AddFixedWindowLimiter(
            policyName: "auth",
            configureOptions: limiterOptions =>
            {
                limiterOptions.PermitLimit = 5;

                limiterOptions.Window =
                    TimeSpan.FromMinutes(1);

                limiterOptions.QueueLimit = 0;

                limiterOptions.AutoReplenishment =
                    true;
            });

        options.AddFixedWindowLimiter(
            policyName: "ai",
            configureOptions: limiterOptions =>
            {
                limiterOptions.PermitLimit = 3;

                limiterOptions.Window =
                    TimeSpan.FromMinutes(1);

                limiterOptions.QueueLimit = 0;

                limiterOptions.AutoReplenishment =
                    true;
            });
    });
}

// ----------------------------
// Email Settings
// ----------------------------
builder.Services.Configure<EmailSettings>(
    builder.Configuration.GetSection(
        "EmailSettings"));

// ----------------------------
// PostgreSQL
// ----------------------------
if (!builder.Environment.IsEnvironment("Testing"))
{
    builder.Services.AddDbContext<AppDbContext>(
        options =>
            options.UseNpgsql(defaultConnection));
}

// ----------------------------
// JWT Services
// ----------------------------
builder.Services.AddScoped<JwtService>();
builder.Services.AddScoped<PasswordService>();

// ----------------------------
// CORS
// ----------------------------
builder.Services.AddCors(options =>
{
    options.AddPolicy(
        "AllowVueFrontend",
        policy =>
        {
            policy
                .WithOrigins(
                    "http://localhost:5173",
                    "https://skill-job-ai-platform.vercel.app")
                .AllowAnyHeader()
                .AllowAnyMethod();
        });
});

// ----------------------------
// JWT Authentication
// ----------------------------
var jwtKey =
    builder.Configuration["Jwt:Key"];

if (string.IsNullOrWhiteSpace(jwtKey))
{
    throw new InvalidOperationException(
        "Die Konfiguration 'Jwt:Key' fehlt. " +
        "Der JWT-Schlüssel muss über User Secrets " +
        "oder eine Umgebungsvariable konfiguriert werden.");
}

if (Encoding.UTF8.GetByteCount(jwtKey) < 32)
{
    throw new InvalidOperationException(
        "Die Konfiguration 'Jwt:Key' muss " +
        "mindestens 32 Bytes lang sein.");
}

var jwtIssuer =
    builder.Configuration["Jwt:Issuer"]
    ?? throw new InvalidOperationException(
        "Die Konfiguration 'Jwt:Issuer' fehlt.");

var jwtAudience =
    builder.Configuration["Jwt:Audience"]
    ?? throw new InvalidOperationException(
        "Die Konfiguration 'Jwt:Audience' fehlt.");

builder.Services
    .AddAuthentication(
        JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters =
            new TokenValidationParameters
            {
                ValidateIssuer = true,
                ValidateAudience = true,
                ValidateLifetime = true,
                ValidateIssuerSigningKey = true,

                ValidIssuer = jwtIssuer,
                ValidAudience = jwtAudience,

                IssuerSigningKey =
                    new SymmetricSecurityKey(
                        Encoding.UTF8.GetBytes(
                            jwtKey)),

                ClockSkew = TimeSpan.Zero
            };
    });

// ----------------------------
// Authorization
// ----------------------------
builder.Services.AddAuthorization();

// ----------------------------
// Swagger
// ----------------------------
builder.Services.AddEndpointsApiExplorer();

builder.Services.AddSwaggerGen(options =>
{
    options.AddSecurityDefinition(
        "Bearer",
        new OpenApiSecurityScheme
        {
            Name = "Authorization",
            Type = SecuritySchemeType.Http,
            Scheme = "bearer",
            BearerFormat = "JWT",
            In = ParameterLocation.Header,
            Description =
                "JWT Token eingeben"
        });

    options.AddSecurityRequirement(
        new OpenApiSecurityRequirement
        {
            {
                new OpenApiSecurityScheme
                {
                    Reference =
                        new OpenApiReference
                        {
                            Type =
                                ReferenceType
                                    .SecurityScheme,

                            Id = "Bearer"
                        }
                },
                Array.Empty<string>()
            }
        });
});

// ----------------------------
// App
// ----------------------------
var app = builder.Build();

// ----------------------------
// Middleware
// ----------------------------

// Muss möglichst früh registriert werden,
// damit Fehler aus der restlichen Pipeline
// abgefangen werden.
app.UseMiddleware<
    GlobalExceptionMiddleware>();

// Swagger nur lokal und in Tests aktivieren.
if (app.Environment.IsDevelopment() ||
    app.Environment.IsEnvironment("Testing"))
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

// HSTS nur außerhalb von Development
// und Testing aktivieren.
if (!app.Environment.IsDevelopment() &&
    !app.Environment.IsEnvironment("Testing"))
{
    app.UseHsts();
}

// HTTP-Anfragen auf HTTPS weiterleiten.
app.UseHttpsRedirection();

// Öffentliche Dateien aus wwwroot.
app.UseStaticFiles();

app.UseCors(
    "AllowVueFrontend");

app.UseRateLimiter();

app.UseSerilogRequestLogging();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.MapHealthChecks(
    "/health");

// ----------------------------
// Start Application
// ----------------------------
try
{
    Log.Information(
        "SkillJobAI API wird gestartet...");

    app.Run();
}
catch (Exception exception)
{
    Log.Fatal(
        exception,
        "Die Anwendung wurde unerwartet beendet.");
}
finally
{
    Log.CloseAndFlush();
}

// Ermöglicht WebApplicationFactory<Program>
// im Integrationstest-Projekt.
public partial class Program
{
}