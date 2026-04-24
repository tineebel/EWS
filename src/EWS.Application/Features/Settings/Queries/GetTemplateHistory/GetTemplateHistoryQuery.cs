using EWS.Application.Common.Models;
using MediatR;

namespace EWS.Application.Features.Settings.Queries.GetTemplateHistory;

public record GetTemplateHistoryQuery(int TemplateId) : IRequest<Result<List<TemplateAuditDto>>>;

public record TemplateAuditDto(
    long AuditId,
    int Version,
    string ChangeType,
    string ChangedBy,
    DateTime ChangedAt,
    string SnapshotJson,
    string? ChangeNote);
