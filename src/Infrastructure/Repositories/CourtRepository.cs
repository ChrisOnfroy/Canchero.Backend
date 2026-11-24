using Application.Common;
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
        GeoBounds? bounds,
        CancellationToken cancellationToken)
    {
        var activeVenues = context.Venues.AsNoTracking().Where(venue => venue.IsActive);
        if (bounds is not null)
        {
            activeVenues = activeVenues.Where(venue =>
                venue.Coordinates.Latitude >= bounds.MinLatitude
                && venue.Coordinates.Latitude <= bounds.MaxLatitude);
            if (bounds.MinLongitude is { } minLongitude && bounds.MaxLongitude is { } maxLongitude)
            {
                activeVenues = activeVenues.Where(venue =>
                    venue.Coordinates.Longitude >= minLongitude
                    && venue.Coordinates.Longitude <= maxLongitude);
            }
        }

        var query =
            from court in context.Courts.AsNoTracking()
            join venue in activeVenues on court.VenueId equals venue.Id
            where court.Status == CourtStatus.Active
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
