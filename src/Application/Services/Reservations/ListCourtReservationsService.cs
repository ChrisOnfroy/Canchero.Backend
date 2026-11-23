using Application.Common;
using Application.DTOs.Reservations.Requests;
using Application.DTOs.Reservations.Responses;
using Application.Errors;
using Application.Interfaces.Repositories;
using Application.Interfaces.Services.Reservations;
using Application.Mappers;
using Application.Services.Common;
using FluentValidation;

namespace Application.Services.Reservations;

/// <summary>
/// Obtiene una página de reservas de una cancha para su propietario o un administrador.
/// </summary>
public sealed class ListCourtReservationsService(
    IUserRepository users,
    IReservationRepository reservations,
    ICourtRepository courts,
    IVenueRepository venues,
    IValidator<ListCourtReservationsRequest> validator) : IListCourtReservationsService
{
    public async Task<PagedResult<ReservationResponse>> ExecuteAsync(
        ListCourtReservationsRequest request,
        CancellationToken cancellationToken = default)
    {
        await validator.ValidateAndThrowAsync(request, cancellationToken);
        var actor = await ServiceAuthorization.GetActiveActorAsync(users, request.ActorUserId, cancellationToken);
        var court = await courts.GetByIdAsync(request.CourtId, cancellationToken)
            ?? throw new NotFoundException("No se encontró la cancha solicitada.");
        var venue = await venues.GetByIdAsync(court.VenueId, cancellationToken)
            ?? throw new NotFoundException("No se encontró la sede de la cancha.");
        ServiceAuthorization.EnsureVenueManagement(actor, venue);

        var result = await reservations.GetByCourtAsync(
            court.Id,
            request.Page,
            request.PageSize,
            cancellationToken);
        return new PagedResult<ReservationResponse>(
            result.Items.Select(reservation => reservation.ToResponse()).ToArray(),
            result.Page,
            result.PageSize,
            result.TotalCount);
    }
}
