using EWS.Infrastructure.Persistence;
using EWS.Infrastructure.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using EWS.Application.Common.Interfaces;

namespace EWS.Application.Tests;

public class DocumentNumberServiceTests
{
    [Fact]
    public async Task GenerateAsync_CreatesSequenceAndIncrementsWithinSamePrefixAndYear()
    {
        await using var db = CreateDbContext();
        var service = new DocumentNumberService(db, new FixedClock(new DateTime(2026, 4, 29)));

        var first = await service.GenerateAsync(1001);
        var second = await service.GenerateAsync(1001);

        Assert.Equal("MEMO-2026-00001", first);
        Assert.Equal("MEMO-2026-00002", second);

        var sequence = await db.WorkflowDocumentSequences.SingleAsync();
        Assert.Equal("MEMO", sequence.Prefix);
        Assert.Equal(2026, sequence.Year);
        Assert.Equal(2, sequence.LastNumber);
    }

    [Fact]
    public async Task GenerateAsync_UsesSeparateSequencesForDifferentPrefixes()
    {
        await using var db = CreateDbContext();
        var service = new DocumentNumberService(db, new FixedClock(new DateTime(2026, 4, 29)));

        var memo = await service.GenerateAsync(1001);
        var pcv = await service.GenerateAsync(2001);

        Assert.Equal("MEMO-2026-00001", memo);
        Assert.Equal("PCV-BR-2026-00001", pcv);
        Assert.Equal(2, await db.WorkflowDocumentSequences.CountAsync());
    }

    private static AppDbContext CreateDbContext()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .ConfigureWarnings(w => w.Ignore(InMemoryEventId.TransactionIgnoredWarning))
            .Options;

        return new AppDbContext(options);
    }

    private sealed class FixedClock(DateTime now) : IDateTimeService
    {
        public DateTime Now => now;
    }
}
