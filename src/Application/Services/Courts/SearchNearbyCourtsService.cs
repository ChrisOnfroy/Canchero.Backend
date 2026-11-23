using Application.Common;
using Application.DTOs.Courts.Requests;
using Application.DTOs.Courts.Responses;
using Application.Errors;
using Application.Interfaces.Repositories;
using Application.Interfaces.Services.Courts;
using Application.Services.Common;
using Domain.ValueObjects;
using FluentValidation;

namespace Application.Services.Courts;

/// <summary>
/// Busca canchas por distancia y, opcionalmente, disponibilidad horaria.
/// </summary>
public sealed class SearchNearbyCourtsService(
    IUserRepository users,
    ICourtRepository courts,
    IVenueRepository venues,
    IReservationRepository reservations,
    IValidator<SearchNearbyCourtsRequest> validator) : ISearchNearbyCourtsService
{
    public async Task<PagedResult<NearbyCourtResponse>> ExecuteAsync(
        SearchNearbyCourtsRequest request,
        CancellationToken cancellationToken = default)
    {
        await validator.ValidateAndThrowAsync(request, cancellationToken);
        await ServiceAuthorization.GetActiveActorAsync(users, request.ActorUserId, cancellationToken);

        var coordinates = new Coordinates(request.Latitude, request.Longitude);
        Money? maxPrice = request.MaxPricePerHour is null
            ? null
            : new Money(request.MaxPricePerHour.Value, request.PriceCurrency);
        TimeSlot? slot = request.SlotStart is { } start && request.SlotEnd is { } end
            ? new TimeSlot(start, end)
            : null;

        var matchingCourts = await courts.SearchAsync(
            request.SportType,
            request.SurfaceType,
            request.IsIndoor,
            maxPrice,
            cancellationToken);
        var venueIds = matchingCourts.Select(court => court.VenueId).Distinct().ToArray();
        var matchingVenues = await venues.GetByIdsAsync(venueIds, cancellationToken);
        var venuesById = matchingVenues.ToDictionary(venue => venue.Id);
        var results = new List<(NearbyCourtResponse Response, double Distance, long Price)>();

        foreach (var court in matchingCourts)
        {
            if (!venuesById.TryGetValue(court.VenueId, out var venue))
            {
                throw new NotFoundException("No se encontró la sede de una cancha coincidente.");
            }

            var distance = venue.DistanceTo(coordinates);
            if (distance.Kilometers > request.RadiusKm)
            {
                continue;
            }

            if (slot is not null)
            {
                if (!venue.IsOpenDuring(slot))
                {
                    continue;
                }

                if (await reservations.ExistsActiveConflictAsync(court.Id, slot, cancellationToken))
                {
                    continue;
                }
            }

            long? slotPrice = slot is null ? null : court.CalculatePrice(slot).Amount;
            results.Add((
                new NearbyCourtResponse(
                    court.Id,
                    court.Name,
                    venue.Id,
                    venue.Name,
                    venue.Address.FullAddress,
                    distance.Meters,
                    distance.Kilometers,
                    court.SportType,
                    court.SurfaceType,
                    court.IsIndoor,
                    court.PricePerHour.Amount,
                    court.PricePerHour.Currency,
                    slotPrice),
                distance.Meters,
                court.PricePerHour.Amount));
        }

        var orderedResults = results
            .OrderBy(item => item.Distance)
            .ThenBy(item => item.Price)
            .ToArray();
        var skip = (int)Math.Min((long)(request.Page - 1) * request.PageSize, int.MaxValue);
        var pageItems = orderedResults
            .Skip(skip)
            .Take(request.PageSize)
            .Select(item => item.Response)
            .ToArray();

        return new PagedResult<NearbyCourtResponse>(
            pageItems,
            request.Page,
            request.PageSize,
            orderedResults.Length);
    }
}
