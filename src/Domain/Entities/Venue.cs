using Domain.Commons;
using Domain.Errors;
using Domain.ValueObjects;

namespace Domain.Entities;

/// <summary>
/// Representa una sede deportiva.
/// </summary>
public sealed class Venue : Entity
{
    /// <summary>
    /// Zona horaria (identificador IANA) que se asigna cuando la sede no indica una.
    /// </summary>
    public const string DefaultTimeZoneId = "America/Bogota";

    private Venue()
    {
    }

    public Venue(
        Guid ownerId,
        string name,
        string description,
        Address address,
        Coordinates coordinates,
        PhoneNumber phoneNumber,
        TimeOfDay openingTime,
        TimeOfDay closingTime,
        string? timeZoneId = null)
        : base()
    {
        if (ownerId == Guid.Empty)
        {
            throw new ValidationException("El propietario de la sede es obligatorio.");
        }

        ValidateName(name);
        if (description is null)
        {
            throw new ValidationException("La descripción no puede ser nula.");
        }

        Guard.AgainstMaxLength(description, 500, nameof(description));
        Guard.AgainstNull(address, nameof(address));
        Guard.AgainstNull(coordinates, nameof(coordinates));
        Guard.AgainstNull(phoneNumber, nameof(phoneNumber));
        Guard.AgainstNull(openingTime, nameof(openingTime));
        Guard.AgainstNull(closingTime, nameof(closingTime));
        ValidateSchedule(openingTime, closingTime);
        var normalizedTimeZoneId = NormalizeTimeZoneId(timeZoneId);

        OwnerId = ownerId;
        Name = name.Trim();
        Description = description.Trim();
        Address = address;
        Coordinates = coordinates;
        PhoneNumber = phoneNumber;
        OpeningTime = openingTime;
        ClosingTime = closingTime;
        TimeZoneId = normalizedTimeZoneId;
        IsActive = true;
    }

    public Guid OwnerId { get; private set; }
    public string Name { get; private set; } = null!;
    public string Description { get; private set; } = null!;
    public Address Address { get; private set; } = null!;
    public Coordinates Coordinates { get; private set; } = null!;
    public PhoneNumber PhoneNumber { get; private set; } = null!;
    public TimeOfDay OpeningTime { get; private set; } = null!;
    public TimeOfDay ClosingTime { get; private set; } = null!;

    /// <summary>
    /// Identificador IANA de la zona horaria en la que se interpretan el horario de apertura y cierre.
    /// </summary>
    public string TimeZoneId { get; private set; } = DefaultTimeZoneId;

    public bool IsActive { get; private set; }

    public void Rename(string name)
    {
        ValidateName(name);
        Name = name.Trim();
        Touch(DateTimeOffset.UtcNow);
    }

    public void ChangeDescription(string description)
    {
        if (description is null)
        {
            throw new ValidationException("La descripción no puede ser nula.");
        }

        Guard.AgainstMaxLength(description, 500, nameof(description));
        Description = description.Trim();
        Touch(DateTimeOffset.UtcNow);
    }

    public void Relocate(Address address, Coordinates coordinates)
    {
        Guard.AgainstNull(address, nameof(address));
        Guard.AgainstNull(coordinates, nameof(coordinates));
        Address = address;
        Coordinates = coordinates;
        Touch(DateTimeOffset.UtcNow);
    }

    public void ChangePhone(PhoneNumber phoneNumber)
    {
        Guard.AgainstNull(phoneNumber, nameof(phoneNumber));
        PhoneNumber = phoneNumber;
        Touch(DateTimeOffset.UtcNow);
    }

    public void ChangeSchedule(TimeOfDay openingTime, TimeOfDay closingTime)
    {
        Guard.AgainstNull(openingTime, nameof(openingTime));
        Guard.AgainstNull(closingTime, nameof(closingTime));
        ValidateSchedule(openingTime, closingTime);
        OpeningTime = openingTime;
        ClosingTime = closingTime;
        Touch(DateTimeOffset.UtcNow);
    }

    public void ChangeTimeZone(string timeZoneId)
    {
        TimeZoneId = NormalizeTimeZoneId(timeZoneId);
        Touch(DateTimeOffset.UtcNow);
    }

    public void Activate()
    {
        IsActive = true;
        Touch(DateTimeOffset.UtcNow);
    }

    public void Deactivate()
    {
        IsActive = false;
        Touch(DateTimeOffset.UtcNow);
    }

    public Distance DistanceTo(Coordinates coordinates) => Coordinates.DistanceTo(coordinates);

    /// <summary>
    /// Indica si el intervalo cabe dentro del horario de la sede, evaluado en la zona horaria de la sede
    /// (no en la del servidor ni en el offset con el que el cliente envió las fechas).
    /// </summary>
    public bool IsOpenDuring(TimeSlot timeSlot)
    {
        Guard.AgainstNull(timeSlot, nameof(timeSlot));
        var localStart = ToVenueLocalTime(timeSlot.Start);
        var localEnd = ToVenueLocalTime(timeSlot.End);
        if (localStart.Date != localEnd.Date)
        {
            return false;
        }

        var start = new TimeOfDay(localStart.Hour, localStart.Minute);
        var end = new TimeOfDay(localEnd.Hour, localEnd.Minute);
        return !start.IsBefore(OpeningTime) && !end.IsAfter(ClosingTime);
    }

    /// <summary>
    /// Convierte un instante a la hora local de la sede.
    /// </summary>
    public DateTimeOffset ToVenueLocalTime(DateTimeOffset instant)
        => TimeZoneInfo.ConvertTime(instant, ResolveTimeZone());

    /// <summary>
    /// Obtiene el instante que corresponde a una fecha y hora de pared en la zona horaria de la sede.
    /// </summary>
    public DateTimeOffset AtVenueLocalTime(DateOnly date, TimeOfDay time)
    {
        Guard.AgainstNull(time, nameof(time));
        var timeZone = ResolveTimeZone();
        var wallClock = date.ToDateTime(new TimeOnly(time.Hour, time.Minute), DateTimeKind.Unspecified);
        return new DateTimeOffset(wallClock, timeZone.GetUtcOffset(wallClock));
    }

    private TimeZoneInfo ResolveTimeZone() => TimeZoneInfo.FindSystemTimeZoneById(TimeZoneId);

    private static string NormalizeTimeZoneId(string? timeZoneId)
    {
        var value = string.IsNullOrWhiteSpace(timeZoneId) ? DefaultTimeZoneId : timeZoneId.Trim();
        Guard.AgainstMaxLength(value, 64, nameof(timeZoneId));
        try
        {
            TimeZoneInfo.FindSystemTimeZoneById(value);
        }
        catch (Exception exception) when (exception is TimeZoneNotFoundException or InvalidTimeZoneException)
        {
            throw new ValidationException(
                $"La zona horaria '{value}' no es válida. Usa un identificador IANA, por ejemplo 'America/Bogota'.");
        }

        return value;
    }

    private static void ValidateName(string name)
    {
        Guard.AgainstEmptyString(name, nameof(name));
        Guard.AgainstMaxLength(name.Trim(), 100, nameof(name));
    }

    private static void ValidateSchedule(TimeOfDay openingTime, TimeOfDay closingTime)
    {
        if (!openingTime.IsBefore(closingTime))
        {
            throw new BusinessRuleException("La hora de apertura debe ser anterior a la hora de cierre.");
        }
    }
}
