namespace BusinessLogicLayer.Helpers;

public static class GeoLocationHelper
{
    /// <summary>
    /// Calculates the distance between two points in meters using the Haversine formula.
    /// </summary>
    public static double CalculateDistance(double lat1, double lon1, double lat2, double lon2)
    {
        var R = 6371e3; // metres
        var phi1 = lat1 * Math.PI / 180;
        var phi2 = lat2 * Math.PI / 180;
        var deltaPhi = (lat2 - lat1) * Math.PI / 180;
        var deltaLambda = (lon2 - lon1) * Math.PI / 180;

        var a = Math.Sin(deltaPhi / 2) * Math.Sin(deltaPhi / 2) +
                Math.Cos(phi1) * Math.Cos(phi2) *
                Math.Sin(deltaLambda / 2) * Math.Sin(deltaLambda / 2);
        var c = 2 * Math.Atan2(Math.Sqrt(a), Math.Sqrt(1 - a));

        return R * c; // in meters
    }

    public static string FormatDistance(double meters)
    {
        if (meters >= 1000)
            return $"{Math.Round(meters / 1000, 2)} km";
        return $"{Math.Round(meters, 0)} m";
    }
}
