// File path in project: RideHailing.API/Services/TripService.cs
using Microsoft.AspNetCore.SignalR;
using RideHailing.API.Hubs;
using RideHailing.API.Models;
using RideHailing.API.Repositories;

namespace RideHailing.API.Services;

public interface ITripService
{
    Task<Trip?> BookAsync(Guid customerId, BookTripRequest request);
    Task<Trip?> AcceptAsync(Guid tripId, Guid driverId);
    Task<Trip?> UpdateStatusAsync(Guid tripId, Guid actorId, string newStatus);
    Task<Trip?> CancelAsync(Guid tripId, Guid actorId, string role, string reason);
    Task<PagedResult<Trip>> GetTripHistoryAsync(Guid userId, string role, int page, int pageSize);
    Task<List<NearbyDriverResponse>> GetNearbyDriversAsync(double lat, double lng);
    Task ActivateDueScheduledTripsAsync();
}

public class TripService(
    ITripRepository tripRepo,
    IDriverRepository driverRepo,
    IFareService fareService,
    ISettingsService settings,
    IHubContext<RideHub> hub) : ITripService
{
    private static readonly Dictionary<string, string[]> AllowedTransitions = new()
    {
        ["requested"]   = ["accepted", "cancelled"],
        ["accepted"]    = ["en_route", "cancelled"],
        ["en_route"]    = ["arrived"],
        ["arrived"]     = ["in_progress"],
        ["in_progress"] = ["completed"],
        ["completed"]   = [],
        ["cancelled"]   = []
    };

    public async Task<Trip?> BookAsync(Guid customerId, BookTripRequest req)
    {
        if (req.ScheduledFor.HasValue)
        {
            var featureEnabled = await settings.GetBoolAsync(SettingKeys.FeatureScheduled, false);
            if (!featureEnabled) return null;

            var minLeadMinutes = await settings.GetIntAsync(SettingKeys.OpsScheduledMinLeadMins, 30);
            if (req.ScheduledFor.Value < DateTime.UtcNow.AddMinutes(minLeadMinutes)) return null;
        }

        var estimate = await fareService.EstimateAsync(new FareEstimateRequest(
            req.PickupLatitude, req.PickupLongitude,
            req.DropoffLatitude, req.DropoffLongitude));

        var trip = new Trip
        {
            CustomerId       = customerId,
            PickupLatitude   = req.PickupLatitude,
            PickupLongitude  = req.PickupLongitude,
            DropoffLatitude  = req.DropoffLatitude,
            DropoffLongitude = req.DropoffLongitude,
            PickupAddress    = req.PickupAddress,
            DropoffAddress   = req.DropoffAddress,
            PaymentMethod    = req.PaymentMethod,
            BaseFare         = estimate.BaseFare,
            DistanceFare     = estimate.EstimatedDistanceFare,
            SurgeMultiplier  = estimate.SurgeMultiplier,
            BookingFee       = estimate.BookingFee,
            FareAmount       = estimate.EstimatedTotal,
            // A scheduled trip sits in 'scheduled' (no dispatch) until its
            // time arrives; an immediate booking goes straight to
            // 'requested' as before. See ActivateDueScheduledTripsAsync.
            Status           = req.ScheduledFor.HasValue ? "scheduled" : "requested",
            ScheduledFor     = req.ScheduledFor,
            RequestedAt      = DateTime.UtcNow
        };

        var created = await tripRepo.CreateAsync(trip);

        // Only dispatch to drivers immediately for non-scheduled bookings —
        // a scheduled trip gets notified later once it's activated.
        if (!req.ScheduledFor.HasValue)
            await NotifyDriversOfNewTripAsync(created);

        return created;
    }

    // Extracted from BookAsync so both an immediate booking and a scheduled
    // trip becoming due (see ActivateDueScheduledTripsAsync) notify drivers
    // through the exact same path.
    private async Task NotifyDriversOfNewTripAsync(Trip trip)
    {
        await hub.Clients.Group("available_drivers").SendAsync("NewTripRequest", new
        {
            TripId           = trip.Id,
            PickupAddress    = trip.PickupAddress,
            DropoffAddress   = trip.DropoffAddress,
            PickupLatitude   = trip.PickupLatitude,
            PickupLongitude  = trip.PickupLongitude,
            FareAmount       = trip.FareAmount,
            PaymentMethod    = trip.PaymentMethod
        });
    }

    // ── Scheduled rides (BP §III) ──────────────────────────────
    // Called every 30s by ScheduledTripActivationService. Flips any due
    // 'scheduled' trips to 'requested' and dispatches them to drivers
    // exactly like a fresh immediate booking.
    public async Task ActivateDueScheduledTripsAsync()
    {
        var dueTrips = await tripRepo.GetDueScheduledTripsAsync();
        foreach (var trip in dueTrips)
        {
            await tripRepo.ActivateScheduledTripAsync(trip.Id);
            trip.Status = "requested";
            await NotifyDriversOfNewTripAsync(trip);
        }
    }

    public async Task<Trip?> AcceptAsync(Guid tripId, Guid driverId)
    {
        var trip = await tripRepo.GetByIdAsync(tripId);
        if (trip == null || trip.Status != "requested") return null;

        trip.DriverId   = driverId;
        trip.Status     = "accepted";
        trip.AcceptedAt = DateTime.UtcNow;

        await tripRepo.UpdateAsync(trip);
        await driverRepo.UpdateStatusAsync(driverId, "on_trip");

        await hub.Clients.Group($"user_{trip.CustomerId}").SendAsync("TripAccepted", new
        {
            TripId   = trip.Id,
            DriverId = driverId
        });

        return trip;
    }

    public async Task<Trip?> UpdateStatusAsync(Guid tripId, Guid actorId, string newStatus)
    {
        var trip = await tripRepo.GetByIdAsync(tripId);
        if (trip == null) return null;

        if (!AllowedTransitions.TryGetValue(trip.Status, out var allowed) || !allowed.Contains(newStatus))
            return null;

        trip.Status = newStatus;

        switch (newStatus)
        {
            case "en_route":    trip.EnRouteAt   = DateTime.UtcNow; break;
            case "arrived":     trip.ArrivedAt   = DateTime.UtcNow; break;
            case "in_progress": trip.StartedAt   = DateTime.UtcNow; break;
            case "completed":
                trip.CompletedAt = DateTime.UtcNow;
                if (trip.DriverId.HasValue)
                    await driverRepo.UpdateStatusAsync(trip.DriverId.Value, "available");
                break;
        }

        await tripRepo.UpdateAsync(trip);

        await hub.Clients.Group($"user_{trip.CustomerId}").SendAsync("TripStatusChanged", new
        {
            TripId = trip.Id,
            Status = newStatus
        });

        return trip;
    }

    public async Task<Trip?> CancelAsync(Guid tripId, Guid actorId, string role, string reason)
    {
        var trip = await tripRepo.GetByIdAsync(tripId);
        if (trip == null) return null;
        if (!AllowedTransitions[trip.Status].Contains("cancelled")) return null;

        if (role == "customer" && trip.Status == "accepted")
        {
            var windowMinutes = await settings.GetIntAsync(SettingKeys.OpsCancelWindow, 3);
            if (trip.AcceptedAt.HasValue && DateTime.UtcNow > trip.AcceptedAt.Value.AddMinutes(windowMinutes))
            {
                trip.FareAmount = await settings.GetDecimalAsync(SettingKeys.FareCancellationFee, 25m);
            }
        }

        trip.Status      = "cancelled";
        trip.CancelledBy = role == "admin" ? "system" : role;
        trip.CancelReason = reason;
        trip.CancelledAt = DateTime.UtcNow;

        await tripRepo.UpdateAsync(trip);

        if (trip.DriverId.HasValue)
            await driverRepo.UpdateStatusAsync(trip.DriverId.Value, "available");

        await hub.Clients.Group($"user_{trip.CustomerId}").SendAsync("TripCancelled", new { TripId = tripId });
        if (trip.DriverId.HasValue)
            await hub.Clients.Group($"user_{trip.DriverId}").SendAsync("TripCancelled", new { TripId = tripId });

        return trip;
    }

    public async Task<PagedResult<Trip>> GetTripHistoryAsync(Guid userId, string role, int page, int pageSize)
        => await tripRepo.GetHistoryAsync(userId, role, page, pageSize);

    public async Task<List<NearbyDriverResponse>> GetNearbyDriversAsync(double lat, double lng)
    {
        var radiusKm = await settings.GetIntAsync(SettingKeys.OpsDriverRadius, 5);
        return await driverRepo.GetNearbyAsync(lat, lng, radiusKm);
    }
}