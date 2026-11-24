using Infrastructure.Context;
using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace Presentation.Health;

/// <summary>
/// Comprueba que la API pueda conectarse a PostgreSQL.
/// </summary>
public sealed class DatabaseHealthCheck(AppDbContext context) : IHealthCheck
{
    public async Task<HealthCheckResult> CheckHealthAsync(
        HealthCheckContext healthCheckContext,
        CancellationToken cancellationToken = default)
    {
        try
        {
            return await context.Database.CanConnectAsync(cancellationToken)
                ? HealthCheckResult.Healthy()
                : HealthCheckResult.Unhealthy("No fue posible conectar con la base de datos.");
        }
        catch (Exception exception) when (exception is not OperationCanceledException)
        {
            return HealthCheckResult.Unhealthy("No fue posible conectar con la base de datos.", exception);
        }
    }
}
