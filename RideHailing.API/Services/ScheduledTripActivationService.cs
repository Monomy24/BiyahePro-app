// File path in project: RideHailing.API/Services/ScheduledTripActivationService.cs
// ============================================================
// Services/ScheduledTripActivationService.cs
// Background sweep for scheduled rides (BP §III "Future Product/Service
// Expansion" — Scheduled Ride Booking).
//
// Every 30 seconds, checks for trips with status = 'scheduled' whose
// scheduled_for time has arrived, and flips them to 'requested' so they
// enter the normal driver-dispatch flow — see
// TripService.ActivateDueScheduledTripsAsync().
// ============================================================
namespace RideHailing.API.Services;

public class ScheduledTripActivationService(
    IServiceScopeFactory scopeFactory,
    ILogger<ScheduledTripActivationService> logger) : BackgroundService
{
    private static readonly TimeSpan PollInterval = TimeSpan.FromSeconds(30);

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                // New scope per tick — ITripService's dependencies (DB
                // connections, etc.) are scoped/transient, not singletons,
                // so a hosted service (which is itself a singleton) can't
                // just inject them directly.
                using var scope = scopeFactory.CreateScope();
                var tripService = scope.ServiceProvider.GetRequiredService<ITripService>();
                await tripService.ActivateDueScheduledTripsAsync();
            }
            catch (Exception ex)
            {
                // Never let one bad tick kill the whole background loop —
                // log and try again next interval.
                logger.LogError(ex, "Failed to activate due scheduled trips.");
            }

            await Task.Delay(PollInterval, stoppingToken);
        }
    }
}