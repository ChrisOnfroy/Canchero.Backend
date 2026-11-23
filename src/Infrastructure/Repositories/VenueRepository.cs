using Application.Interfaces.Repositories;
using Domain.Entities;
using Infrastructure.Context;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Repositories;

/// <summary>
/// Implementa las consultas y altas de sedes.
/// </summary>
public sealed class VenueRepository(AppDbContext context) : IVenueRepository
{
    public Task<Venue?> GetByIdAsync(Guid venueId, CancellationToken cancellationToken)
        => context.Venues.FirstOrDefaultAsync(venue => venue.Id == venueId, cancellationToken);

    public async Task<IReadOnlyList<Venue>> GetByIdsAsync(
        IReadOnlyCollection<Guid> venueIds,
        CancellationToken cancellationToken)
        => await context.Venues
            .AsNoTracking()
            .Where(venue => venueIds.Contains(venue.Id))
            .ToListAsync(cancellationToken);

    public async Task<IReadOnlyList<Venue>> GetByOwnerAsync(Guid ownerId, CancellationToken cancellationToken)
        => await context.Venues
            .AsNoTracking()
            .Where(venue => venue.OwnerId == ownerId)
            .OrderBy(venue => venue.Name)
            .ToListAsync(cancellationToken);

    public async Task AddAsync(Venue venue, CancellationToken cancellationToken)
        => await context.Venues.AddAsync(venue, cancellationToken);
}
