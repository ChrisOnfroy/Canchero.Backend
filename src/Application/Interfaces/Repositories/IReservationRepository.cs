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

    Task AddAsync(Reservation reservation, CancellationToken cancellationToken);
}
