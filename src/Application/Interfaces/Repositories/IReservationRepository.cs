using Application.Common;
using Domain.Entities;
using Domain.Enums;
using Domain.ValueObjects;

namespace Application.Interfaces.Repositories;

/// <summary>
/// Define las operaciones de persistencia y consulta de reservas.
/// </summary>
public interface IReservationRepository
{
    Task<Reservation?> GetByIdAsync(Guid reservationId, CancellationToken cancellationToken);

    Task<PagedResult<Reservation>> GetByUserAsync(
        Guid userId,
        int page,
        int pageSize,
        ReservationStatus? status,
        CancellationToken cancellationToken);

    Task<PagedResult<Reservation>> GetByCourtAsync(
        Guid courtId,
        int page,
        int pageSize,
        CancellationToken cancellationToken);

    /// <summary>
    /// Obtiene las reservas activas que se superponen al día local delimitado por ambos offsets.
    /// </summary>
    Task<IReadOnlyList<Reservation>> GetActiveByCourtAndDayAsync(
        Guid courtId,
        DateTimeOffset dayStart,
        DateTimeOffset dayEnd,
        CancellationToken cancellationToken);

    Task<bool> ExistsActiveConflictAsync(
        Guid courtId,
        TimeSlot slot,
        CancellationToken cancellationToken);

    /// <summary>
    /// Obtiene, en una sola consulta, los identificadores de las canchas indicadas que tienen una
    /// reserva activa superpuesta con el intervalo.
    /// </summary>
    Task<IReadOnlySet<Guid>> GetCourtIdsWithActiveConflictAsync(
        IReadOnlyCollection<Guid> courtIds,
        TimeSlot slot,
        CancellationToken cancellationToken);

    /// <summary>
    /// Cuenta las reservas de un usuario que están pendientes de confirmación.
    /// </summary>
    Task<int> CountPendingByUserAsync(Guid userId, CancellationToken cancellationToken);

    Task AddAsync(Reservation reservation, CancellationToken cancellationToken);
}
