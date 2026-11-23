using Domain.Commons;
using Domain.Enums;
using Domain.Errors;
using Domain.ValueObjects;

namespace Domain.Entities;

/// <summary>
/// Representa una reserva de una cancha.
/// </summary>
public sealed class Reservation : Entity
{
    private Reservation()
    {
    }

    public Reservation(Guid userId, Guid courtId, TimeSlot timeSlot, Money totalPrice)
        : base()
    {
        if (userId == Guid.Empty)
        {
            throw new ValidationException("El usuario de la reserva es obligatorio.");
        }

        if (courtId == Guid.Empty)
        {
            throw new ValidationException("La cancha de la reserva es obligatoria.");
        }

        Guard.AgainstNull(timeSlot, nameof(timeSlot));
        Guard.AgainstNull(totalPrice, nameof(totalPrice));
        if (totalPrice.IsZero)
        {
            throw new ValidationException("El precio total debe ser mayor que cero.");
        }

        UserId = userId;
        CourtId = courtId;
        TimeSlot = timeSlot;
        TotalPrice = totalPrice;
        Status = ReservationStatus.Pending;
    }

    public Guid UserId { get; private set; }
    public Guid CourtId { get; private set; }
    public TimeSlot TimeSlot { get; private set; } = null!;
    public Money TotalPrice { get; private set; } = null!;
    public ReservationStatus Status { get; private set; }
    public string? CancellationReason { get; private set; }

    public void Confirm()
    {
        if (Status != ReservationStatus.Pending)
        {
            throw InvalidTransition("confirmar");
        }

        Status = ReservationStatus.Confirmed;
        Touch(DateTimeOffset.UtcNow);
    }

    public void Cancel(string? reason)
    {
        if (Status is not ReservationStatus.Pending and not ReservationStatus.Confirmed)
        {
            throw InvalidTransition("cancelar");
        }

        if (reason is not null)
        {
            Guard.AgainstMaxLength(reason, 300, nameof(reason));
        }

        Status = ReservationStatus.Cancelled;
        CancellationReason = reason?.Trim();
        Touch(DateTimeOffset.UtcNow);
    }

    public void Complete(DateTimeOffset now)
    {
        if (Status != ReservationStatus.Confirmed)
        {
            throw InvalidTransition("completar");
        }

        if (now < TimeSlot.End)
        {
            throw new BusinessRuleException("La reserva solo puede completarse después de finalizar el horario.");
        }

        Status = ReservationStatus.Completed;
        Touch(now);
    }

    public void MarkAsNoShow(DateTimeOffset now)
    {
        if (Status != ReservationStatus.Confirmed)
        {
            throw InvalidTransition("marcar como no presentado");
        }

        if (now < TimeSlot.Start)
        {
            throw new BusinessRuleException("La reserva solo puede marcarse como no presentada después de iniciar.");
        }

        Status = ReservationStatus.NoShow;
        Touch(now);
    }

    public bool IsActive() => Status is ReservationStatus.Pending or ReservationStatus.Confirmed;

    public bool ConflictsWith(TimeSlot other)
    {
        Guard.AgainstNull(other, nameof(other));
        return Status is not ReservationStatus.Cancelled
            && TimeSlot.OverlapsWith(other);
    }

    private BusinessRuleException InvalidTransition(string action)
        => new($"No se puede {action} una reserva en estado {Status}.");
}
