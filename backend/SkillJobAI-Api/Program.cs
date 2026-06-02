using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using SkillJobAI.Api.Data;
using SkillJobAI.Api.Services;

var builder = WebApplication.CreateBuilder(args);

// Controller aktivieren
builder.Services.AddControllers();

// PostgreSQL Datenbank verbinden
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseNpgsql(
        builder.Configuration.GetConnectionString("DefaultConnection")));

// JWT Service registrieren
builder.Services.AddScoped<JwtService>();
builder.Services.AddScoped<PasswordService>();

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowVueFrontend", policy =>
    {
        policy.WithOrigins(
         "http://localhost:5173",
         "https://skill-job-ai-platform.vercel.app"
     )
     .AllowAnyHeader()
     .AllowAnyMethod();
    });
});

// JWT Einstellungen laden
var jwtKey = builder.Configuration["Jwt:Key"]!;

// JWT Authentication konfigurieren
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

// Rollen & Berechtigungen aktivieren
builder.Services.AddAuthorization();

// Swagger aktivieren + JWT Authorize Button
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

var app = builder.Build();

app.UseDeveloperExceptionPage();

app.Use(async (context, next) =>
{
    try
    {
        await next();
    }
    catch (Exception ex)
    {
        Console.WriteLine("FULL ERROR:");
        Console.WriteLine(ex.ToString());
        throw;
    }
});

app.UseSwagger();
app.UseSwaggerUI();

// app.UseHttpsRedirection();

// CORS aktivieren, damit Vue Frontend auf Backend zugreifen darf
app.UseCors("AllowVueFrontend");

// JWT aktivieren
app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();

//if (app.Environment.IsDevelopment()){}