using System.Net.Http.Headers;
using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.IdentityModel.Tokens;
using SkillJobAI.Api.Data;
using SkillJobAI.Api.Entities;
using SkillJobAI.Api.Services;

namespace SkillJobAI.Api.Tests.Integration;

public sealed class CustomWebApplicationFactory
    : WebApplicationFactory<Program>
{
    private readonly string _databaseName =
        $"SkillJobAI_IntegrationTests_{Guid.NewGuid():N}";

    protected override void ConfigureWebHost(
        IWebHostBuilder builder
    )
    {
        builder.UseEnvironment("Testing");

        /*
         * Diese Werte werden sehr früh in die
         * Host-Konfiguration geschrieben.
         *
         * Dadurch stehen sie bereits zur Verfügung,
         * wenn Program.cs den JWT-Schlüssel ausliest.
         */
        builder.UseSetting(
            "Jwt:Key",
            JwtTestHelper.JwtKey
        );

        builder.UseSetting(
            "Jwt:Issuer",
            JwtTestHelper.JwtIssuer
        );

        builder.UseSetting(
            "Jwt:Audience",
            JwtTestHelper.JwtAudience
        );

        builder.ConfigureAppConfiguration(
            (_, configurationBuilder) =>
            {
                var configurationValues =
                    new Dictionary<string, string?>
                    {
                        ["ConnectionStrings:DefaultConnection"] =
                            "Host=localhost;" +
                            "Database=SkillJobAITests;" +
                            "Username=test;" +
                            "Password=test",

                        ["Jwt:Key"] =
                            JwtTestHelper.JwtKey,

                        ["Jwt:Issuer"] =
                            JwtTestHelper.JwtIssuer,

                        ["Jwt:Audience"] =
                            JwtTestHelper.JwtAudience,

                        ["Jwt:ExpiresInMinutes"] =
                            "15",

                        ["Jwt:RefreshTokenExpiresInDays"] =
                            "30",

                        ["Frontend:BaseUrl"] =
                            "http://localhost:5173",

                        ["EmailSettings:Host"] =
                            "localhost",

                        ["EmailSettings:Port"] =
                            "1025",

                        ["EmailSettings:FromEmail"] =
                            "tests@skilljobai.local",

                        ["EmailSettings:FromName"] =
                            "SkillJobAI Tests",

                        ["EmailSettings:Username"] =
                            "test",

                        ["EmailSettings:Password"] =
                            "test"
                    };

                configurationBuilder
                    .AddInMemoryCollection(
                        configurationValues
                    );
            }
        );

        builder.ConfigureServices(
            services =>
            {
                ReplaceDatabase(services);
                ConfigureJwtValidation(services);
                ReplaceEmailService(services);
            }
        );
    }

    public HttpClient CreateAuthenticatedClient(
        AppUser user
    )
    {
        var client = CreateClient();

        var token =
            JwtTestHelper.GenerateToken(user);

        client.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue(
                JwtBearerDefaults.AuthenticationScheme,
                token
            );

        return client;
    }

    public FakeEmailService GetFakeEmailService()
    {
        return Services
            .GetRequiredService<FakeEmailService>();
    }

    public async Task ResetDatabaseAsync()
    {
        using var scope =
            Services.CreateScope();

        var context =
            scope.ServiceProvider
                .GetRequiredService<AppDbContext>();

        await context.Database
            .EnsureDeletedAsync();

        await context.Database
            .EnsureCreatedAsync();

        GetFakeEmailService().Clear();
    }

    public async Task SeedAsync(
        Func<AppDbContext, Task> seedAction
    )
    {
        using var scope =
            Services.CreateScope();

        var context =
            scope.ServiceProvider
                .GetRequiredService<AppDbContext>();

        await context.Database
            .EnsureCreatedAsync();

        await seedAction(context);

        await context.SaveChangesAsync();
    }

    public async Task ExecuteDbContextAsync(
        Func<AppDbContext, Task> action
    )
    {
        using var scope =
            Services.CreateScope();

        var context =
            scope.ServiceProvider
                .GetRequiredService<AppDbContext>();

        await action(context);
    }

    private void ReplaceDatabase(
        IServiceCollection services
    )
    {
        services.RemoveAll<AppDbContext>();

        services.RemoveAll<
            DbContextOptions<AppDbContext>>();

        services.RemoveAll<
            IDbContextOptionsConfiguration<AppDbContext>>();

        services.AddDbContext<AppDbContext>(
            options =>
                options
                    .UseInMemoryDatabase(
                        _databaseName
                    )
                    .ConfigureWarnings(
                        warnings =>
                            warnings.Ignore(
                                InMemoryEventId
                                    .TransactionIgnoredWarning
                            )
                    )
        );
    }

    private static void ConfigureJwtValidation(
        IServiceCollection services
    )
    {
        services.PostConfigure<JwtBearerOptions>(
            JwtBearerDefaults.AuthenticationScheme,
            options =>
            {
                options.TokenValidationParameters =
                    new TokenValidationParameters
                    {
                        ValidateIssuer = true,
                        ValidateAudience = true,
                        ValidateLifetime = true,
                        ValidateIssuerSigningKey = true,

                        ValidIssuer =
                            JwtTestHelper.JwtIssuer,

                        ValidAudience =
                            JwtTestHelper.JwtAudience,

                        IssuerSigningKey =
                            new SymmetricSecurityKey(
                                Encoding.UTF8.GetBytes(
                                    JwtTestHelper.JwtKey
                                )
                            ),

                        ClockSkew = TimeSpan.Zero
                    };
            }
        );
    }

    private static void ReplaceEmailService(
        IServiceCollection services
    )
    {
        services.RemoveAll<IEmailService>();

        services.AddSingleton<
            FakeEmailService>();

        services.AddSingleton<IEmailService>(
            serviceProvider =>
                serviceProvider
                    .GetRequiredService<
                        FakeEmailService>()
        );
    }
}

