namespace Infrastructure.Settings;

/// <summary>
/// Configuración del proveedor Nominatim.
/// </summary>
public sealed class GeocodingSettings
{
    public string BaseUrl { get; set; } = string.Empty;
    public string UserAgent { get; set; } = string.Empty;
    public string CountryCodes { get; set; } = "co";
    public int CacheMinutes { get; set; } = 60;
    public int TimeoutSeconds { get; set; } = 10;
}
