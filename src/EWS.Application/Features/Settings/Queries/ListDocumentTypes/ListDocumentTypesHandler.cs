using EWS.Application.Common.Interfaces;
using EWS.Application.Common.Models;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace EWS.Application.Features.Settings.Queries.ListDocumentTypes;

public class ListDocumentTypesHandler(IAppDbContext db)
    : IRequestHandler<ListDocumentTypesQuery, Result<List<DocumentTypeDto>>>
{
    public async Task<Result<List<DocumentTypeDto>>> Handle(ListDocumentTypesQuery req, CancellationToken ct)
    {
        var q = db.DocumentTypes
            .Include(d => d.Templates)
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(req.Search))
            q = q.Where(d => d.DocName.Contains(req.Search) || d.DocCode.ToString().Contains(req.Search));

        if (req.IsActive.HasValue)
            q = q.Where(d => d.IsActive == req.IsActive.Value);

        var result = await q.OrderBy(d => d.DocCode)
            .Select(d => new DocumentTypeDto(
                d.DocumentTypeId,
                d.DocCode,
                d.DocName,
                d.DocNameEn,
                d.Description,
                d.Category,
                d.IsActive,
                d.Templates.Count))
            .ToListAsync(ct);

        return Result<List<DocumentTypeDto>>.Success(result);
    }
}
