using Domain.Entities;

namespace Application.Interfaces.Repositories;

/// <summary>
/// Define las operaciones de persistencia de sedes.
/// </summary>
public interface IVenueRepository
{
    Task<Venue?> GetByIdAsync(Guid venueId, CancellationToken cancellationToken);
    Task<IReadOnlyList<Venue>> GetByIdsAsync(IReadOnlyCollection<Guid> venueIds, CancellationToken cancellationToken);
    Task<IReadOnlyList<Venue>> GetByOwnerAsync(Guid ownerId, CancellationToken cancellationToken);
    Task AddAsync(Venue venue, CancellationToken cancellationToken);
}
