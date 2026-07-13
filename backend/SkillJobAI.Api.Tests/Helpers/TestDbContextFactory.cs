using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using SkillJobAI.Api.Data;

namespace SkillJobAI.Api.Tests.Helpers;

public static class TestDbContextFactory
{
    public static AppDbContext Create(
        bool ignoreTransactionWarnings = false)
    {
        var databaseName =
            $"SkillJobAI_Tests_{Guid.NewGuid():N}";

        var optionsBuilder =
            new DbContextOptionsBuilder<AppDbContext>()
                .UseInMemoryDatabase(databaseName)
                .EnableDetailedErrors()
                .EnableSensitiveDataLogging();

        if (ignoreTransactionWarnings)
        {
            optionsBuilder.ConfigureWarnings(
                warnings =>
                    warnings.Ignore(
                        InMemoryEventId
                            .TransactionIgnoredWarning));
        }

        return new AppDbContext(
            optionsBuilder.Options);
    }
}