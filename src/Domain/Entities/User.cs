using Domain.Commons;
using Domain.Enums;
using Domain.ValueObjects;

namespace Domain.Entities;

/// <summary>
/// Representa un usuario del sistema.
/// </summary>
public sealed class User : Entity
{
    private User()
    {
    }

    public User(PersonName name, Email email, PhoneNumber? phoneNumber, UserRole role = UserRole.Customer)
        : base()
    {
        Guard.AgainstNull(name, nameof(name));
        Guard.AgainstNull(email, nameof(email));
        Name = name;
        Email = email;
        PhoneNumber = phoneNumber;
        Role = role;
        IsActive = true;
    }

    public PersonName Name { get; private set; } = null!;
    public Email Email { get; private set; } = null!;
    public PhoneNumber? PhoneNumber { get; private set; }
    public UserRole Role { get; private set; }
    public bool IsActive { get; private set; }

    public void ChangeName(PersonName name)
    {
        Guard.AgainstNull(name, nameof(name));
        Name = name;
        Touch(DateTimeOffset.UtcNow);
    }

    public void ChangeEmail(Email email)
    {
        Guard.AgainstNull(email, nameof(email));
        Email = email;
        Touch(DateTimeOffset.UtcNow);
    }

    public void ChangePhone(PhoneNumber? phoneNumber)
    {
        PhoneNumber = phoneNumber;
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

    public bool CanManageVenues() => IsActive && Role is UserRole.VenueOwner or UserRole.Admin;
    public bool CanReserve() => IsActive && Role is not UserRole.Admin;
}
