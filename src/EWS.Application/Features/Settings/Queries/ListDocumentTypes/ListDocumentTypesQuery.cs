using EWS.Application.Common.Models;
using MediatR;

namespace EWS.Application.Features.Settings.Queries.ListDocumentTypes;

public record ListDocumentTypesQuery(string? Search, bool? IsActive)
    : IRequest<Result<List<DocumentTypeDto>>>;

public record DocumentTypeDto(
    int DocumentTypeId,
    int DocCode,
    string DocName,
    string? DocNameEn,
    string? Description,
    string Category,
    bool IsActive,
    int TemplateCount);
