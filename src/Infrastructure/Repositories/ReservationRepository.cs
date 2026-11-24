using Application.Common;
using Application.Interfaces.Repositories;
using Domain.Entities;
using Domain.Enums;
using Domain.ValueObjects;
using Infrastructure.Context;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Repositories;

/// <summary>
/// Implementa las consultas y altas de reservas.
/// </summary>
public sealed class ReservationRepository(AppDbContext context) : IReservationRepository
{
    public Task<Reservation?> GetByIdAsync(Guid reservationId, CancellationToken cancellationToken)
        => context.Reservations.FirstOrDefaultAsync(
            reservation => reservation.Id == reservationId,
            cancellationToken);

    public async Task<PagedResult<Reservation>> GetByUserAsync(
        Guid userId,
        int page,
        int pageSize,
        ReservationStatus? status,
        CancellationToken cancellationToken)
    {
        var query = context.Reservations.AsNoTracking().Where(reservation => reservation.UserId == userId);
        if (status.HasValue)
        {
            query = query.Where(reservation => reservation.Status == status.Value);
        }

        var totalCount = await query.CountAsync(cancellationToken);
        var skip = (int)Math.Min((long)(page - 1) * pageSize, int.MaxValue);
        var items = await query
            .OrderByDescending(reservation => reservation.TimeSlot.Start)
            .Skip(skip)
            .Take(pageSize)
            .ToListAsync(cancellationToken);
        return new PagedResult<Reservation>(items, page, pageSize, totalCount);
    }

    public async Task<PagedResult<Reservation>> GetByCourtAsync(
        Guid courtId,
        int page,
        int pageSize,
        CancellationToken cancellationToken)
    {
        var query = context.Reservations.AsNoTracking().Where(reservation => reservation.CourtId == courtId);
        var totalCount = await query.CountAsync(cancellationToken);
        var skip = (int)Math.Min((long)(page - 1) * pageSize, int.MaxValue);
        var items = await query
            .OrderBy(reservation => reservation.TimeSlot.Start)
            .Skip(skip)
            .Take(pageSize)
            .ToListAsync(cancellationToken);
        return new PagedResult<Reservation>(items, page, pageSize, totalCount);
    }

    public async Task<IReadOnlyList<Reservation>> GetActiveByCourtAndDayAsync(
        Guid courtId,
        DateTimeOffset dayStart,
        DateTimeOffset dayEnd,
        CancellationToken cancellationToken)
        => await context.Reservations
            .AsNoTracking()
            .Where(reservation =>
                reservation.CourtId == courtId
                && (reservation.Status == ReservationStatus.Pending
                    || reservation.Status == ReservationStatus.Confirmed)
                && reservation.TimeSlot.Start < dayEnd
                && reservation.TimeSlot.End > dayStart)
            .OrderBy(reservation => reservation.TimeSlot.Start)
            .ToListAsync(cancellationToken);

    public Task<bool> ExistsActiveConflictAsync(
        Guid courtId,
        TimeSlot slot,
        CancellationToken cancellationToken)
        => context.Reservations.AsNoTracking().AnyAsync(reservation =>
            reservation.CourtId == courtId
            && (reservation.Status == ReservationStatus.Pending
                || reservation.Status == ReservationStatus.Confirmed)
            && reservation.TimeSlot.Start < slot.End
            && reservation.TimeSlot.End > slot.Start,
            cancellationToken);

    public async Task<IReadOnlySet<Guid>> GetCourtIdsWithActiveConflictAsync(
        IReadOnlyCollection<Guid> courtIds,
        TimeSlot slot,
        CancellationToken cancellationToken)
    {
        if (courtIds.Count == 0)
        {
            return new HashSet<Guid>();
        }

        var conflicting = await context.Reservations
            .AsNoTracking()
            .Where(reservation =>
                courtIds.Contains(reservation.CourtId)
                && (reservation.Status == ReservationStatus.Pending
                    || reservation.Status == ReservationStatus.Confirmed)
                && reservation.TimeSlot.Start < slot.End
                && reservation.TimeSlot.End > slot.Start)
            .Select(reservation => reservation.CourtId)
            .Distinct()
            .ToListAsync(cancellationToken);
        return conflicting.ToHashSet();
    }

    public Task<int> CountPendingByUserAsync(Guid userId, CancellationToken cancellationToken)
        => context.Reservations.AsNoTracking().CountAsync(
            reservation => reservation.UserId == userId
                && reservation.Status == ReservationStatus.Pending,
            cancellationToken);

    public async Task AddAsync(Reservation reservation, CancellationToken cancellationToken)
        => await context.Reservations.AddAsync(reservation, cancellationToken);
}
