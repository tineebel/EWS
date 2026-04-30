using EWS.Application.Common.Models;
using MediatR;

namespace EWS.Application.Features.Settings.Commands.DeleteDelegation;

public record DeleteDelegationCommand(int DelegationId, string ChangedBy)
    : IRequest<Result>;
