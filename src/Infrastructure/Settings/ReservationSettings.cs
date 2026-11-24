namespace Infrastructure.Settings;

/// <summary>
/// Configuración del vencimiento automático de reservas pendientes.
/// </summary>
public sealed class ReservationSettings
{
    /// <summary>
    /// Minutos que una reserva puede permanecer pendiente antes de cancelarse automáticamente.
    /// </summary>
    public int PendingExpirationMinutes { get; set; } = 30;

    /// <summary>
    /// Segundos entre cada revisión de reservas vencidas.
    /// </summary>
    public int SweepIntervalSeconds { get; set; } = 60;
}
