namespace Infrastructure.Services;

/// <summary>
/// Indica que el proveedor externo de geocodificación no respondió correctamente.
/// </summary>
public sealed class GeocodingProviderException : Exception
{
    public GeocodingProviderException(string message)
        : base(message)
    {
    }

    public GeocodingProviderException(string message, Exception innerException)
        : base(message, innerException)
    {
    }
}
