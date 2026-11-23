using Domain.Entities;
using Domain.Enums;
using Domain.ValueObjects;

namespace Application.Interfaces.Repositories;

/// <summary>
/// Define las operaciones de persistencia y búsqueda de canchas.
/// </summary>
public interface ICourtRepository
{
    Task<Court?> GetByIdAsync(Guid courtId, CancellationToken cancellationToken);
    Task<IReadOnlyList<Court>> GetByVenueAsync(Guid venueId, CancellationToken cancellationToken);

    /// <summary>
    /// Busca canchas activas pertenecientes a sedes activas según los filtros indicados.
    /// </summary>
    Task<IReadOnlyList<Court>> SearchAsync(
        SportType? sportType,
        SurfaceType? surfaceType,
        bool? isIndoor,
        Money? maxPricePerHour,
        CancellationToken cancellationToken);

    Task AddAsync(Court court, CancellationToken cancellationToken);
}
