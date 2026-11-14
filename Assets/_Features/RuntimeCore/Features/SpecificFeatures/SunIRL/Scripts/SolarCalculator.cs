using System;

/// <summary>
/// Sun position from the NOAA solar calculator (declination and equation of time included),
/// accurate to about a minute for sunrise/sunset in the years this app works with.
/// </summary>
public static class SolarCalculator {
    /// <summary>Sunrise/sunset: the sun's upper edge on the horizon, atmospheric refraction included.</summary>
    public const double SunriseElevation = -0.833;
    /// <summary>Dawn/dusk (civil twilight): the sun 6° below the horizon.</summary>
    public const double CivilTwilightElevation = -6.0;

    private const double Deg2Rad = Math.PI / 180.0;
    private const double Rad2Deg = 180.0 / Math.PI;

    /// <summary>
    /// Geometric elevation above the horizon and azimuth (clockwise from north), both in degrees.
    /// </summary>
    public static void Position(DateTime utc, double latitude, double longitude, out double elevationDeg, out double azimuthDeg) {
        double julianCentury = (JulianDay(utc) - 2451545.0) / 36525.0;
        SunParameters(julianCentury, out double declination, out double equationOfTimeMinutes);

        // True solar time: 720 min = solar noon (4 min per degree of longitude)
        double trueSolarMinutes = utc.TimeOfDay.TotalMinutes + equationOfTimeMinutes + 4.0 * longitude;
        trueSolarMinutes = (trueSolarMinutes % 1440.0 + 1440.0) % 1440.0;
        double hourAngle = (trueSolarMinutes / 4.0 - 180.0) * Deg2Rad; // < 0 morning, > 0 afternoon

        double lat = latitude * Deg2Rad;
        double sinElevation = Math.Sin(lat) * Math.Sin(declination) +
                              Math.Cos(lat) * Math.Cos(declination) * Math.Cos(hourAngle);
        double elevation = Math.Asin(Clamp(sinElevation, -1.0, 1.0));

        double cosAzimuth = (Math.Sin(declination) * Math.Cos(lat) -
                             Math.Cos(declination) * Math.Sin(lat) * Math.Cos(hourAngle)) /
                            Math.Max(1e-6, Math.Cos(elevation));
        double azimuth = Math.Acos(Clamp(cosAzimuth, -1.0, 1.0));
        if (hourAngle > 0.0) azimuth = 2.0 * Math.PI - azimuth; // afternoon sun is in the west

        elevationDeg = elevation * Rad2Deg;
        azimuthDeg = azimuth * Rad2Deg;
    }

    /// <summary>
    /// Finds dawn, sunrise, sunset and dusk in one local day by scanning the sun's elevation.
    /// elevationAtMinute gets minutes after local midnight (0–1440). Scanning the same elevation
    /// the scene is lit with keeps the times in step with the lighting, also across a DST switch.
    /// An event that doesn't happen that day (polar day/night, white nights) is null.
    /// </summary>
    public static SunTimes FindSunTimes(Func<double, double> elevationAtMinute) {
        const int stepMinutes = 5;
        const int samples = 1440 / stepMinutes;

        var elevations = new double[samples + 1];
        for (int i = 0; i <= samples; i++) {
            elevations[i] = elevationAtMinute(i * stepMinutes);
        }

        return new SunTimes {
            Dawn = FindCrossing(CivilTwilightElevation, rising: true),
            Sunrise = FindCrossing(SunriseElevation, rising: true),
            Sunset = FindCrossing(SunriseElevation, rising: false),
            Dusk = FindCrossing(CivilTwilightElevation, rising: false),
        };

        // First crossing upwards / last crossing downwards, refined by bisection to well under a second
        TimeSpan? FindCrossing(double threshold, bool rising) {
            for (int n = 0; n < samples; n++) {
                int i = rising ? n : samples - 1 - n;
                double a = elevations[i] - threshold;
                double b = elevations[i + 1] - threshold;
                bool crosses = rising ? (a < 0.0 && b >= 0.0) : (a >= 0.0 && b < 0.0);
                if (!crosses) continue;

                double lo = i * stepMinutes, hi = lo + stepMinutes;
                for (int k = 0; k < 20; k++) {
                    double mid = 0.5 * (lo + hi);
                    bool above = elevationAtMinute(mid) >= threshold;
                    if (above == rising) hi = mid; else lo = mid;
                }
                return TimeSpan.FromMinutes(0.5 * (lo + hi));
            }
            return null;
        }
    }

    private static double JulianDay(DateTime utc) {
        return utc.ToOADate() + 2415018.5; // OLE dates count days from 30 Dec 1899
    }

    private static void SunParameters(double t, out double declination, out double equationOfTimeMinutes) {
        double meanLongitude = ((280.46646 + t * (36000.76983 + t * 0.0003032)) % 360.0) * Deg2Rad;
        double meanAnomaly = (357.52911 + t * (35999.05029 - 0.0001537 * t)) * Deg2Rad;
        double eccentricity = 0.016708634 - t * (0.000042037 + 0.0000001267 * t);

        double equationOfCenter =
            Math.Sin(meanAnomaly) * (1.914602 - t * (0.004817 + 0.000014 * t)) +
            Math.Sin(2.0 * meanAnomaly) * (0.019993 - 0.000101 * t) +
            Math.Sin(3.0 * meanAnomaly) * 0.000289;
        double trueLongitude = meanLongitude * Rad2Deg + equationOfCenter;

        double omega = (125.04 - 1934.136 * t) * Deg2Rad;
        double apparentLongitude = (trueLongitude - 0.00569 - 0.00478 * Math.Sin(omega)) * Deg2Rad;

        double meanObliquity = 23.0 + (26.0 + (21.448 - t * (46.815 + t * (0.00059 - t * 0.001813))) / 60.0) / 60.0;
        double obliquity = (meanObliquity + 0.00256 * Math.Cos(omega)) * Deg2Rad;

        declination = Math.Asin(Math.Sin(obliquity) * Math.Sin(apparentLongitude));

        double y = Math.Tan(obliquity / 2.0);
        y *= y;
        double eot = y * Math.Sin(2.0 * meanLongitude)
                   - 2.0 * eccentricity * Math.Sin(meanAnomaly)
                   + 4.0 * eccentricity * y * Math.Sin(meanAnomaly) * Math.Cos(2.0 * meanLongitude)
                   - 0.5 * y * y * Math.Sin(4.0 * meanLongitude)
                   - 1.25 * eccentricity * eccentricity * Math.Sin(2.0 * meanAnomaly);
        equationOfTimeMinutes = 4.0 * eot * Rad2Deg;
    }

    private static double Clamp(double value, double min, double max) {
        return value < min ? min : value > max ? max : value;
    }
}

/// <summary>
/// Local clock times of one day's twilight and sun events; null when the event doesn't happen that day.
/// </summary>
public struct SunTimes {
    public TimeSpan? Dawn;    // civil dawn, sun at -6°
    public TimeSpan? Sunrise;
    public TimeSpan? Sunset;
    public TimeSpan? Dusk;    // civil dusk, sun at -6°
}
