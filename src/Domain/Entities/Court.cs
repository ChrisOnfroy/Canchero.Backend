using Domain.Commons;
using Domain.Enums;
using Domain.Errors;
using Domain.ValueObjects;

namespace Domain.Entities;

/// <summary>
/// Representa una cancha deportiva.
/// </summary>
public sealed class Court : Entity
{
    private Court()
    {
    }

    public Court(
        Guid venueId,
        string name,
        SportType sportType,
        SurfaceType surfaceType,
        Capacity capacity,
        Money pricePerHour,
        bool isIndoor)
        : base()
    {
        if (venueId == Guid.Empty)
        {
            throw new ValidationException("La sede de la cancha es obligatoria.");
        }

        ValidateName(name);
        Guard.AgainstNull(capacity, nameof(capacity));
        Guard.AgainstNull(pricePerHour, nameof(pricePerHour));
        if (pricePerHour.IsZero)
        {
            throw new ValidationException("El precio por hora debe ser mayor que cero.");
        }

        VenueId = venueId;
        Name = name.Trim();
        SportType = sportType;
        SurfaceType = surfaceType;
        Capacity = capacity;
        PricePerHour = pricePerHour;
        IsIndoor = isIndoor;
        Status = CourtStatus.Active;
    }

    public Guid VenueId { get; private set; }
    public string Name { get; private set; } = null!;
    public SportType SportType { get; private set; }
    public SurfaceType SurfaceType { get; private set; }
    public Capacity Capacity { get; private set; } = null!;
    public Money PricePerHour { get; private set; } = null!;
    public bool IsIndoor { get; private set; }
    public CourtStatus Status { get; private set; }

    public void Rename(string name)
    {
        ValidateName(name);
        Name = name.Trim();
        Touch(DateTimeOffset.UtcNow);
    }

    public void ChangeSurface(SurfaceType surfaceType)
    {
        SurfaceType = surfaceType;
        Touch(DateTimeOffset.UtcNow);
    }

    public void ChangeCapacity(Capacity capacity)
    {
        Guard.AgainstNull(capacity, nameof(capacity));
        Capacity = capacity;
        Touch(DateTimeOffset.UtcNow);
    }

    public void ChangePrice(Money pricePerHour)
    {
        Guard.AgainstNull(pricePerHour, nameof(pricePerHour));
        if (pricePerHour.IsZero)
        {
            throw new ValidationException("El precio por hora debe ser mayor que cero.");
        }

        PricePerHour = pricePerHour;
        Touch(DateTimeOffset.UtcNow);
    }

    public void Activate()
    {
        Status = CourtStatus.Active;
        Touch(DateTimeOffset.UtcNow);
    }

    public void Deactivate()
    {
        Status = CourtStatus.Inactive;
        Touch(DateTimeOffset.UtcNow);
    }

    public void SendToMaintenance()
    {
        Status = CourtStatus.Maintenance;
        Touch(DateTimeOffset.UtcNow);
    }

    public bool IsBookable() => Status == CourtStatus.Active;

    public Money CalculatePrice(TimeSlot timeSlot)
    {
        Guard.AgainstNull(timeSlot, nameof(timeSlot));
        var duration = timeSlot.DurationInMinutes;
        if (duration <= 0)
        {
            throw new ValidationException("La duración de la reserva debe ser mayor que cero.");
        }

        var proportion = (decimal)(duration / 60);
        return PricePerHour.Multiply(proportion);
    }

    private static void ValidateName(string name)
    {
        Guard.AgainstEmptyString(name, nameof(name));
        Guard.AgainstMaxLength(name.Trim(), 80, nameof(name));
    }
}
