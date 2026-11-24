using Application.Common;
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
    /// Busca canchas activas pertenecientes a sedes activas según los filtros indicados. Si se indica
    /// <paramref name="bounds"/>, solo se consideran las sedes dentro de ese rectángulo geográfico.
    /// </summary>
    Task<IReadOnlyList<Court>> SearchAsync(
        SportType? sportType,
        SurfaceType? surfaceType,
        bool? isIndoor,
        Money? maxPricePerHour,
        GeoBounds? bounds,
        CancellationToken cancellationToken);

    Task AddAsync(Court court, CancellationToken cancellationToken);
}
