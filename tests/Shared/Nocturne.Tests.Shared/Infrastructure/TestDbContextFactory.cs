using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.EntityFrameworkCore.InMemory.Storage.Internal;
using Nocturne.Infrastructure.Data;

namespace Nocturne.Tests.Shared.Infrastructure;

public static class TestDbContextFactory
{
    public static NocturneDbContext CreateInMemoryContext(string? databaseName = null)
    {
        var options = new DbContextOptionsBuilder<NocturneDbContext>()
            .UseInMemoryDatabase(databaseName ?? $"nocturne_tests_{Guid.NewGuid()}")
            .ConfigureWarnings(w => w.Ignore(InMemoryEventId.TransactionIgnoredWarning))
            .EnableSensitiveDataLogging()
            .Options;

        var context = new NocturneDbContext(options);
        context.Database.EnsureCreated();
        return context;
    }
}
