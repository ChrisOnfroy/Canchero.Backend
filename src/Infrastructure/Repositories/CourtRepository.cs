using Application.Interfaces.Repositories;
using Domain.Entities;
using Domain.Enums;
using Domain.ValueObjects;
using Infrastructure.Context;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Repositories;

/// <summary>
/// Implementa las consultas y altas de canchas.
/// </summary>
public sealed class CourtRepository(AppDbContext context) : ICourtRepository
{
    public Task<Court?> GetByIdAsync(Guid courtId, CancellationToken cancellationToken)
        => context.Courts.FirstOrDefaultAsync(court => court.Id == courtId, cancellationToken);

    public async Task<IReadOnlyList<Court>> GetByVenueAsync(Guid venueId, CancellationToken cancellationToken)
        => await context.Courts
            .AsNoTracking()
            .Where(court => court.VenueId == venueId)
            .OrderBy(court => court.Name)
            .ToListAsync(cancellationToken);

    public async Task<IReadOnlyList<Court>> SearchAsync(
        SportType? sportType,
        SurfaceType? surfaceType,
        bool? isIndoor,
        Money? maxPricePerHour,
        CancellationToken cancellationToken)
    {
        var query =
            from court in context.Courts.AsNoTracking()
            join venue in context.Venues.AsNoTracking() on court.VenueId equals venue.Id
            where court.Status == CourtStatus.Active && venue.IsActive
            select court;

        if (sportType.HasValue)
        {
            query = query.Where(court => court.SportType == sportType.Value);
        }

        if (surfaceType.HasValue)
        {
            query = query.Where(court => court.SurfaceType == surfaceType.Value);
        }

        if (isIndoor.HasValue)
        {
            query = query.Where(court => court.IsIndoor == isIndoor.Value);
        }

        if (maxPricePerHour is not null)
        {
            query = query.Where(court =>
                court.PricePerHour.Currency == maxPricePerHour.Currency
                && court.PricePerHour.Amount <= maxPricePerHour.Amount);
        }

        return await query.ToListAsync(cancellationToken);
    }

    public async Task AddAsync(Court court, CancellationToken cancellationToken)
        => await context.Courts.AddAsync(court, cancellationToken);
}
