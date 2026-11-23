using System.Globalization;
using System.Text.Json;
using Application.DTOs.Geocoding;
using Application.Interfaces.Geocoding;
using Domain.Errors;
using Domain.ValueObjects;
using Infrastructure.Settings;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Options;

namespace Infrastructure.Services;

/// <summary>
/// Resuelve lugares con Nominatim y almacena temporalmente los resultados.
/// </summary>
public sealed class NominatimGeocodingService(
    HttpClient httpClient,
    IMemoryCache cache,
    IOptions<GeocodingSettings> options) : IGeocodingService
{
    private static readonly SemaphoreSlim RequestGate = new(1, 1);
    private static DateTimeOffset _nextRequestAt;
    private readonly GeocodingSettings _settings = options.Value;

    public async Task<GeocodingResult?> GeocodeAsync(
        string query,
        CancellationToken cancellationToken = default)
    {
        var normalizedQuery = query.Trim().ToLowerInvariant();
        var cacheKey = $"nominatim:{normalizedQuery}";
        if (cache.TryGetValue(cacheKey, out GeocodingResult? cachedResult))
        {
            return cachedResult;
        }

        var result = await QueryProviderAsync(normalizedQuery, cancellationToken);
        var cacheDuration = result is null
            ? TimeSpan.FromSeconds(Math.Min(_settings.CacheMinutes * 60, 30))
            : TimeSpan.FromMinutes(_settings.CacheMinutes);
        cache.Set(cacheKey, result, cacheDuration);
        return result;
    }

    private async Task<GeocodingResult?> QueryProviderAsync(
        string query,
        CancellationToken cancellationToken)
    {
        await RequestGate.WaitAsync(cancellationToken);
        try
        {
            var delay = _nextRequestAt - DateTimeOffset.UtcNow;
            if (delay > TimeSpan.Zero)
            {
                await Task.Delay(delay, cancellationToken);
            }

            var endpoint = new Uri(
                new Uri(_settings.BaseUrl.TrimEnd('/') + "/"),
                $"search?format=jsonv2&limit=1&countrycodes={Uri.EscapeDataString(_settings.CountryCodes)}&q={Uri.EscapeDataString(query)}");
            using var request = new HttpRequestMessage(HttpMethod.Get, endpoint);
            request.Headers.UserAgent.ParseAdd(_settings.UserAgent);

            _nextRequestAt = DateTimeOffset.UtcNow.AddSeconds(1);
            using var response = await httpClient.SendAsync(request, cancellationToken);
            if (!response.IsSuccessStatusCode)
            {
                throw new GeocodingProviderException(
                    $"Nominatim respondió con el estado HTTP {(int)response.StatusCode}.");
            }

            await using var stream = await response.Content.ReadAsStreamAsync(cancellationToken);
            using var document = await JsonDocument.ParseAsync(
                stream,
                cancellationToken: cancellationToken);
            if (document.RootElement.ValueKind != JsonValueKind.Array)
            {
                throw new GeocodingProviderException("Nominatim devolvió una respuesta con formato inválido.");
            }

            var firstResult = document.RootElement.EnumerateArray().FirstOrDefault();
            if (firstResult.ValueKind == JsonValueKind.Undefined)
            {
                return null;
            }

            var latitude = double.Parse(
                firstResult.GetProperty("lat").GetString()!,
                CultureInfo.InvariantCulture);
            var longitude = double.Parse(
                firstResult.GetProperty("lon").GetString()!,
                CultureInfo.InvariantCulture);
            var formattedAddress = firstResult.GetProperty("display_name").GetString();
            if (string.IsNullOrWhiteSpace(formattedAddress))
            {
                throw new GeocodingProviderException("Nominatim no devolvió una dirección normalizada.");
            }

            return new GeocodingResult(new Coordinates(latitude, longitude), formattedAddress);
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            throw;
        }
        catch (GeocodingProviderException)
        {
            throw;
        }
        catch (Exception exception) when (
            exception is HttpRequestException
            or TaskCanceledException
            or JsonException
            or KeyNotFoundException
            or InvalidOperationException
            or FormatException
            or DomainException)
        {
            throw new GeocodingProviderException(
                "No fue posible obtener la ubicación desde Nominatim.",
                exception);
        }
        finally
        {
            RequestGate.Release();
        }
    }
}
