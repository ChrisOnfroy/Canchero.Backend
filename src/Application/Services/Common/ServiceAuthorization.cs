using Application.Errors;
using Application.Interfaces.Repositories;
using Domain.Entities;
using Domain.Enums;

namespace Application.Services.Common;

/// <summary>
/// Centraliza la carga de actores y la autorización común de los servicios.
/// </summary>
internal static class ServiceAuthorization
{
    public static async Task<User> GetActiveActorAsync(
        IUserRepository users,
        Guid actorUserId,
        CancellationToken cancellationToken)
    {
        var actor = await users.GetByIdAsync(actorUserId, cancellationToken)
            ?? throw new NotFoundException("No se encontró el usuario que ejecuta la acción.");
        if (!actor.IsActive)
        {
            throw new ForbiddenException("El usuario que ejecuta la acción está inactivo.");
        }

        return actor;
    }

    public static bool IsAdmin(User actor) => actor.Role == UserRole.Admin;

    public static void EnsureVenueManagement(User actor, Venue venue)
    {
        if (!actor.CanManageVenues() || (!IsAdmin(actor) && venue.OwnerId != actor.Id))
        {
            throw new ForbiddenException("No tienes permiso para administrar esta sede.");
        }
    }

    public static void EnsureVenueOwner(User actor, Venue venue)
    {
        if (!actor.CanManageVenues() || venue.OwnerId != actor.Id)
        {
            throw new ForbiddenException("Solo el propietario puede administrar las canchas de esta sede.");
        }
    }

    public static async Task<Reservation> GetReservationForVenueManagerAsync(
        User actor,
        Guid reservationId,
        IReservationRepository reservations,
        ICourtRepository courts,
        IVenueRepository venues,
        CancellationToken cancellationToken)
    {
        var reservation = await reservations.GetByIdAsync(reservationId, cancellationToken)
            ?? throw new NotFoundException("No se encontró la reserva solicitada.");
        var court = await courts.GetByIdAsync(reservation.CourtId, cancellationToken)
            ?? throw new NotFoundException("No se encontró la cancha de la reserva.");
        var venue = await venues.GetByIdAsync(court.VenueId, cancellationToken)
            ?? throw new NotFoundException("No se encontró la sede de la cancha.");
        EnsureVenueManagement(actor, venue);
        return reservation;
    }
}
