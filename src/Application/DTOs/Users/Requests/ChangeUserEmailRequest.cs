namespace Application.DTOs.Users.Requests;

public sealed record ChangeUserEmailRequest(Guid ActorUserId, Guid UserId, string NewEmail);
