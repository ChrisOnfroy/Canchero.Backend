namespace Application.DTOs.Users.Requests;

/// <summary>
/// Solicita los datos de un usuario.
/// </summary>
public sealed record GetUserByIdRequest(Guid ActorUserId, Guid UserId);
