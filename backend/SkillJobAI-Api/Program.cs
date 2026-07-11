using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
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


// ----------------------------
// Services registrieren
// ----------------------------
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<IJobService, JobService>();
builder.Services.AddScoped<ICompanyService, CompanyService>();
builder.Services.AddScoped<ICourseService, CourseService>();
builder.Services.AddScoped<ILessonService, LessonService>();
builder.Services.AddScoped<IApplicationService, ApplicationService>();
builder.Services.AddScoped<IUserService, UserService>();
builder.Services.AddScoped<IEnrollmentService, EnrollmentService>();
builder.Services.AddScoped<ICertificateService, CertificateService>();
builder.Services.AddScoped<ISkillGapService, SkillGapService>();
builder.Services.AddScoped<IProgressService, ProgressService>();
builder.Services.AddScoped<IUserSkillService, UserSkillService>();
builder.Services.AddScoped<IJobSkillService, JobSkillService>();
builder.Services.AddScoped<ICompanyMemberService, CompanyMemberService>();
builder.Services.AddScoped<ICandidateDashboardService, CandidateDashboardService>();
builder.Services.AddScoped<ISkillService, SkillService>();
builder.Services.AddScoped<ICourseSkillService, CourseSkillService>();
builder.Services.AddScoped<IAdminService, AdminService>();
builder.Services.AddScoped<IRecruiterService, RecruiterService>();
builder.Services.AddScoped<IRecruiterCandidateService, RecruiterCandidateService>();
builder.Services.AddScoped<ICareerRoadmapService, CareerRoadmapService>();
builder.Services.AddScoped<IAiService, AiService>();
builder.Services.AddScoped<IEmailService, EmailService>();
builder.Services.AddScoped<IFileStorageService, FileStorageService>();
builder.Services.AddScoped<IApplicationMatchingService, ApplicationMatchingService>();


// ----------------------------
// Email Settings
// ----------------------------
builder.Services.Configure<EmailSettings>(
    builder.Configuration.GetSection("EmailSettings"));


// ----------------------------
// PostgreSQL
// ----------------------------
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseNpgsql(
        builder.Configuration.GetConnectionString("DefaultConnection")));


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
    options.AddPolicy("AllowVueFrontend", policy =>
    {
        policy.WithOrigins(
                "http://localhost:5173",
                "https://skill-job-ai-platform.vercel.app")
            .AllowAnyHeader()
            .AllowAnyMethod();
    });
});


// ----------------------------
// JWT Authentication
// ----------------------------
var jwtKey = builder.Configuration["Jwt:Key"]!;

builder.Services
    .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters =
            new TokenValidationParameters
            {
                ValidateIssuer = true,
                ValidateAudience = true,
                ValidateLifetime = true,
                ValidateIssuerSigningKey = true,
                ValidIssuer = builder.Configuration["Jwt:Issuer"],
                ValidAudience = builder.Configuration["Jwt:Audience"],
                IssuerSigningKey = new SymmetricSecurityKey(
                    Encoding.UTF8.GetBytes(jwtKey))
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
    options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = SecuritySchemeType.Http,
        Scheme = "bearer",
        BearerFormat = "JWT",
        In = ParameterLocation.Header,
        Description = "JWT Token eingeben"
    });

    options.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
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
// damit Fehler aus der restlichen Pipeline abgefangen werden.
app.UseMiddleware<GlobalExceptionMiddleware>();

app.UseSwagger();
app.UseSwaggerUI();

app.UseStaticFiles();

// app.UseHttpsRedirection();

app.UseCors("AllowVueFrontend");

// Serilog Request Logging
app.UseSerilogRequestLogging();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();


// ----------------------------
// Start Application
// ----------------------------
try
{
    Log.Information("SkillJobAI API wird gestartet...");

    app.Run();
}
catch (Exception ex)
{
    Log.Fatal(
        ex,
        "Die Anwendung wurde unerwartet beendet.");
}
finally
{
    Log.CloseAndFlush();
}