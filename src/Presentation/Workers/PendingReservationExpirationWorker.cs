using Application.Interfaces.Common;
using Domain.Entities;
using Domain.Enums;
using Infrastructure.Context;
using Infrastructure.Settings;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace Presentation.Workers;

/// <summary>
/// Cancela periódicamente las reservas que siguen pendientes pasado el tiempo de espera o cuyo
/// horario ya comenzó, para que dejen de bloquear la agenda de la cancha.
/// </summary>
public sealed class PendingReservationExpirationWorker(
    IServiceScopeFactory scopeFactory,
    IDateTimeProvider dateTimeProvider,
    IOptions<ReservationSettings> options,
    ILogger<PendingReservationExpirationWorker> logger) : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var interval = TimeSpan.FromSeconds(options.Value.SweepIntervalSeconds);
        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await ExpireAsync(stoppingToken);
            }
            catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
            {
                break;
            }
            catch (Exception exception)
            {
                // No se detiene el worker: el siguiente ciclo vuelve a intentarlo.
                logger.LogError(exception, "No fue posible expirar las reservas pendientes.");
            }

            try
            {
                await Task.Delay(interval, stoppingToken);
            }
            catch (OperationCanceledException)
            {
                break;
            }
        }
    }

    private async Task ExpireAsync(CancellationToken cancellationToken)
    {
        await using var scope = scopeFactory.CreateAsyncScope();
        var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var now = dateTimeProvider.UtcNow;
        var cutoff = now - TimeSpan.FromMinutes(options.Value.PendingExpirationMinutes);

        var expired = await context.Reservations
            .Where(reservation =>
                reservation.Status == ReservationStatus.Pending
                && (reservation.CreatedAt <= cutoff || reservation.TimeSlot.Start <= now))
            .ExecuteUpdateAsync(
                updates => updates
                    .SetProperty(reservation => reservation.Status, ReservationStatus.Cancelled)
                    .SetProperty(reservation => reservation.CancellationReason, Reservation.PendingExpirationReason)
                    .SetProperty(reservation => reservation.UpdatedAt, now),
                cancellationToken);

        if (expired > 0)
        {
            logger.LogInformation("Se cancelaron {Count} reservas pendientes vencidas.", expired);
        }
    }
}
