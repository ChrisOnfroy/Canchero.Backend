using Microsoft.AspNetCore.Identity;

namespace Infrastructure.Identity;

/// <summary>
/// Cuenta de autenticación de un usuario de Canchero.
/// </summary>
public sealed class ApplicationUser : IdentityUser<Guid>
{
}
