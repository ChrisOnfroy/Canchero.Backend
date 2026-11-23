namespace Application.DTOs.Courts.Requests;

/// <summary>
/// Solicita los datos de una cancha.
/// </summary>
public sealed record GetCourtByIdRequest(Guid ActorUserId, Guid CourtId);
