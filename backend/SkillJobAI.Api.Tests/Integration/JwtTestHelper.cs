using Microsoft.Extensions.Configuration;
using SkillJobAI.Api.Entities;
using SkillJobAI.Api.Services;

namespace SkillJobAI.Api.Tests.Integration;

public static class JwtTestHelper
{
    public const string JwtKey =
        "ThisIsAnIntegrationTestJwtKeyThatIsLongEnoughForHmacSha256";

    public const string JwtIssuer =
        "SkillJobAI.IntegrationTests";

    public const string JwtAudience =
        "SkillJobAI.IntegrationTests";

    public static string GenerateToken(
        AppUser user)
    {
        var configurationValues =
            new Dictionary<string, string?>
            {
                ["Jwt:Key"] =
                    JwtKey,

                ["Jwt:Issuer"] =
                    JwtIssuer,

                ["Jwt:Audience"] =
                    JwtAudience,

                ["Jwt:ExpiresInMinutes"] =
                    "15"
            };

        var configuration =
            new ConfigurationBuilder()
                .AddInMemoryCollection(
                    configurationValues)
                .Build();

        var jwtService =
            new JwtService(configuration);

        return jwtService.GenerateToken(user);
    }
}