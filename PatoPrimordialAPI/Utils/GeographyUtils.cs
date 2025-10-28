using System;

namespace PatoPrimordialAPI.Utils;

public static class GeographyUtils
{
    private const double EarthRadiusMeters = 6371000d;

    public static double CalcularDistanciaMetros(double lat1, double lon1, double lat2, double lon2)
    {
        var dLat = GrausParaRad(lat2 - lat1);
        var dLon = GrausParaRad(lon2 - lon1);

        var a = Math.Sin(dLat / 2) * Math.Sin(dLat / 2) +
                Math.Cos(GrausParaRad(lat1)) * Math.Cos(GrausParaRad(lat2)) *
                Math.Sin(dLon / 2) * Math.Sin(dLon / 2);
        var c = 2 * Math.Atan2(Math.Sqrt(a), Math.Sqrt(1 - a));
        return EarthRadiusMeters * c;
    }

    private static double GrausParaRad(double graus) => graus * Math.PI / 180d;
}
