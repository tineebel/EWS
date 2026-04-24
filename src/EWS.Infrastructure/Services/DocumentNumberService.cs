using EWS.Application.Common.Interfaces;
using EWS.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace EWS.Infrastructure.Services;

public class DocumentNumberService(AppDbContext db, IDateTimeService clock) : IDocumentNumberService
{
    private static readonly Dictionary<int, string> DocPrefixes = new()
    {
        { 1001, "MEMO" }, { 1002, "MEMO-DOA" },
        { 2001, "PCV-BR" }, { 2002, "PCV-HO" },
        { 2003, "PCR-BR" }, { 2004, "PCR-HO" },
        { 2005, "ADV-BR" }, { 2006, "ADV-HO" },
        { 2007, "ADC-BR" }, { 2008, "ADC-HO" },
        { 2009, "EXP-BR" }, { 2010, "EXP-HO" },
        { 2101, "WOF" },
        { 5001, "SIS-ACC" }, { 5002, "SIS-PR" }, { 5003, "SIS-PO-BR" },
        { 5004, "SIS-PO-HO" }, { 5005, "SIS-D365" }, { 5006, "SIS-TO" },
        { 6001, "CTR" }, { 6002, "DEP" }, { 6003, "CUS" },
    };

    public async Task<string> GenerateAsync(int docCode, CancellationToken ct = default)
    {
        var prefix = DocPrefixes.GetValueOrDefault(docCode, $"DOC{docCode}");
        var year = clock.Now.Year;
        var yearPrefix = $"{prefix}-{year}-";

        var count = await db.WorkflowInstances
            .Where(x => x.DocumentNo.StartsWith(yearPrefix))
            .CountAsync(ct);

        return $"{yearPrefix}{(count + 1):D5}";
    }
}
