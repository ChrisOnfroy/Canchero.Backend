using Application.DTOs.Courts.Requests;
using Application.DTOs.Courts.Responses;
using Application.Errors;
using Application.Interfaces.Repositories;
using Application.Interfaces.Services.Courts;
using Application.Mappers;
using Application.Services.Common;
using Domain.Enums;
using FluentValidation;

namespace Application.Services.Courts;

/// <summary>
/// Cambia el estado operativo de una cancha.
/// </summary>
public sealed class ChangeCourtStatusService(
    IUserRepository users,
    IVenueRepository venues,
    ICourtRepository courts,
    IUnitOfWork unitOfWork,
    IValidator<ChangeCourtStatusRequest> validator) : IChangeCourtStatusService
{
    public async Task<CourtResponse> ExecuteAsync(
        ChangeCourtStatusRequest request,
        CancellationToken cancellationToken = default)
    {
        await validator.ValidateAndThrowAsync(request, cancellationToken);
        var actor = await ServiceAuthorization.GetActiveActorAsync(users, request.ActorUserId, cancellationToken);
        var court = await courts.GetByIdAsync(request.CourtId, cancellationToken)
            ?? throw new NotFoundException("No se encontró la cancha solicitada.");
        var venue = await venues.GetByIdAsync(court.VenueId, cancellationToken)
            ?? throw new NotFoundException("No se encontró la sede de la cancha.");
        ServiceAuthorization.EnsureVenueManagement(actor, venue);

        switch (request.Status)
        {
            case CourtStatus.Active:
                court.Activate();
                break;
            case CourtStatus.Inactive:
                court.Deactivate();
                break;
            case CourtStatus.Maintenance:
                court.SendToMaintenance();
                break;
            default:
                throw new Domain.Errors.ValidationException("El estado de la cancha no es válido.");
        }

        await unitOfWork.SaveChangesAsync(cancellationToken);
        return court.ToResponse();
    }
}
