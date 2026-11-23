namespace Infrastructure.Settings;

/// <summary>
/// Configuración opcional de creación del usuario administrador inicial.
/// </summary>
public sealed class AdminSeedSettings
{
    public string? Email { get; set; }
    public string? Password { get; set; }
    public string? FirstName { get; set; }
    public string? LastName { get; set; }
}
