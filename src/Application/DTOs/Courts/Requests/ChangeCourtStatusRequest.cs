using Domain.Enums;

namespace Application.DTOs.Courts.Requests;

/// <summary>
/// Solicita cambiar el estado operativo de una cancha.
/// </summary>
public sealed record ChangeCourtStatusRequest(Guid ActorUserId, Guid CourtId, CourtStatus Status);
