using RideHailing.API.Models;

namespace RideHailing.API.Services;

public interface IFareService
{
    Task<FareEstimateResponse> EstimateAsync(FareEstimateRequest request);
    Task<(decimal total, decimal baseFare, decimal distFare, decimal timeFare, decimal surge)>
        CalculateAsync(double distanceKm, int durationMinutes);
}

public class FareService(ISettingsService settings) : IFareService
{
    public async Task<FareEstimateResponse> EstimateAsync(FareEstimateRequest req)
    {
        var distKm = HaversineKm(req.PickupLatitude, req.PickupLongitude,
                                 req.DropoffLatitude, req.DropoffLongitude);

        // Estimate travel duration based on typical city traffic speeds
        var estimatedMinutes = (int)(distKm / 25.0 * 60);

        var (total, baseFare, distFare, timeFare, surge) =
            await CalculateAsync(distKm, estimatedMinutes);

        return new FareEstimateResponse(
            BaseFare: baseFare,
            EstimatedDistanceFare: distFare,
            EstimatedTotal: total,
            SurgeMultiplier: surge,
            EstimatedDistanceKm: Math.Round(distKm, 2),
            EstimatedMinutes: estimatedMinutes
        );
    }

    public async Task<(decimal total, decimal baseFare, decimal distFare, decimal timeFare, decimal surge)>
        CalculateAsync(double distanceKm, int durationMinutes)
    {
        var baseFare   = await settings.GetDecimalAsync(SettingKeys.FareBase, 40m);
        var perKm      = await settings.GetDecimalAsync(SettingKeys.FarePerKm, 12m);
        var perMinute  = await settings.GetDecimalAsync(SettingKeys.FarePerMinute, 2.5m);
        var minimum    = await settings.GetDecimalAsync(SettingKeys.FareMinimum, 80m);
        var surgeOn    = await settings.GetBoolAsync(SettingKeys.SurgeEnabled, false);
        var surgeMulti = await settings.GetDecimalAsync(SettingKeys.SurgeMultiplier, 1.0m);

        var distFare = (decimal)distanceKm * perKm;
        var timeFare = durationMinutes * perMinute;
        var surge    = surgeOn ? surgeMulti : 1.0m;

        var subtotal = (baseFare + distFare + timeFare) * surge;
        var total    = Math.Max(subtotal, minimum);

        return (
            total:    Math.Round(total, 2),
            baseFare: Math.Round(baseFare, 2),
            distFare: Math.Round(distFare, 2),
            timeFare: Math.Round(timeFare, 2),
            surge:    surge
        );
    }

    private static double HaversineKm(double lat1, double lon1, double lat2, double lon2)
    {
        const double R = 6371; // Earth's radius in kilometers
        var dLat = ToRad(lat2 - lat1);
        var dLon = ToRad(lon2 - lon1);
        var a = Math.Sin(dLat / 2) * Math.Sin(dLat / 2)
              + Math.Cos(ToRad(lat1)) * Math.Cos(ToRad(lat2))
              * Math.Sin(dLon / 2) * Math.Sin(dLon / 2);
        return R * 2 * Math.Atan2(Math.Sqrt(a), Math.Sqrt(1 - a));
    }

    private static double ToRad(double deg) => deg * Math.PI / 180;
}
