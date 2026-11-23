using Domain.Commons;
using Domain.Errors;
using Domain.ValueObjects;

namespace Domain.Entities;

/// <summary>
/// Representa una sede deportiva.
/// </summary>
public sealed class Venue : Entity
{
    public Venue(
        Guid ownerId,
        string name,
        string description,
        Address address,
        Coordinates coordinates,
        PhoneNumber phoneNumber,
        TimeOfDay openingTime,
        TimeOfDay closingTime)
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

        OwnerId = ownerId;
        Name = name.Trim();
        Description = description.Trim();
        Address = address;
        Coordinates = coordinates;
        PhoneNumber = phoneNumber;
        OpeningTime = openingTime;
        ClosingTime = closingTime;
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

    public bool IsOpenDuring(TimeSlot timeSlot)
    {
        Guard.AgainstNull(timeSlot, nameof(timeSlot));
        if (!timeSlot.IsSameDay())
        {
            return false;
        }

        var start = new TimeOfDay(timeSlot.Start.LocalDateTime.Hour, timeSlot.Start.LocalDateTime.Minute);
        var end = new TimeOfDay(timeSlot.End.LocalDateTime.Hour, timeSlot.End.LocalDateTime.Minute);
        return !start.IsBefore(OpeningTime) && !end.IsAfter(ClosingTime);
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
