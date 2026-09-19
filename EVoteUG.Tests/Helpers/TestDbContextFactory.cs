using EVoteUG.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace EVoteUG.Tests.Helpers;

public static class TestDbContextFactory
{
    public static EVoteUGDbContext CreateInMemoryDbContext()
    {
        var options = new DbContextOptionsBuilder<EVoteUGDbContext>()
            .UseInMemoryDatabase(databaseName: $"EVoteUG_TestDb_{Guid.NewGuid():N}")
            .ConfigureWarnings(w => w.Ignore(Microsoft.EntityFrameworkCore.Diagnostics.InMemoryEventId.TransactionIgnoredWarning))
            .Options;

        return new EVoteUGDbContext(options);
    }
}
