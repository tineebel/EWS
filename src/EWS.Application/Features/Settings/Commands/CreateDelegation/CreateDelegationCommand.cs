using EWS.Application.Common.Models;
using MediatR;

namespace EWS.Application.Features.Settings.Commands.CreateDelegation;

public record CreateDelegationCommand(
    string FromPositionCode,
    string ToPositionCode,
    DateTime StartDate,
    DateTime EndDate,
    string? Reason,
    bool IsActive,
    string ChangedBy) : IRequest<Result<int>>;
