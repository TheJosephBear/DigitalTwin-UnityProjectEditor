# Sun position calculation

The scene's sun light ([SunIRL.cs](Scripts/SunIRL.cs)) and the time slider's daylight track
([DaylightTrackColors.cs](Scripts/DaylightTrackColors.cs)) both get the sun's position from
[SolarCalculator.cs](Scripts/SolarCalculator.cs). This keeps the scene's light and the track's
colours in step: the light comes on at the sunrise the track shows.

This note compares the original calculation with the current one (changed November 2026).

## What stayed the same

- **Local time to UTC:** done by `SunIRL.GetUtcOffsetHours`, which knows when summer time starts and ends.
- **Elevation and azimuth:** the same spherical-astronomy formulas, from declination, hour angle and latitude:
  ```
  sin(elevation) = sin(lat)·sin(decl) + cos(lat)·cos(decl)·cos(hourAngle)
  cos(azimuth)   = (sin(decl)·cos(lat) − cos(decl)·sin(lat)·cos(hourAngle)) / cos(elevation)
  ```
  Azimuth is measured clockwise from true north. In the afternoon (`hourAngle > 0`) it is `360° − azimuth`.
- **Light rotation:** `Quaternion.Euler(elevation, azimuth + 180°, 0)`. The +180° makes the light point away from the sun.
- **Elevation used for the light:** the geometric elevation, with no correction for the atmosphere.

## Original calculation

The original calculation was inline in `SunIRL.CalculateSunQuaternion`, using `float`:

```csharp
int dayOfYear = dateTime.DayOfYear;
float utcHours = localHours - GetUtcOffsetHours(dateTime);
float solarTimeHours = utcHours + Longitude / 15f;                       // no equation of time
float declination = 23.45° · sin(360/365 · (dayOfYear + 284));           // Cooper's approximation
float hourAngle = (solarTimeHours − 12) · 15°;
```

It has two simplifications:

1. **No equation of time.** Solar time was taken as UTC plus longitude ÷ 15. That assumes the sun
   reaches its highest point at the same clock time every day. Because Earth's orbit is an ellipse
   and its axis is tilted, the real sun runs ahead of or behind that even schedule over the year.
   Leaving this out moved the whole day, including sunrise, noon, sunset and the shadow directions,
   by up to about 16 minutes.
2. **Rough declination.** Cooper's formula assumes a circular orbit and a 365-day year, ignores
   leap years, and keeps one value for the whole day. It can be off by roughly a degree at some
   times of year, which moves sunrise and sunset at Prague's latitude by a few minutes.

## Current calculation (NOAA)

`SolarCalculator.Position` follows the NOAA Solar Calculator, using `double`.

```
julianCentury = (JulianDay(utc) − 2451545.0) / 36525
  → mean longitude, mean anomaly, orbital eccentricity
  → equation of centre → true longitude → apparent longitude (nutation)
  → obliquity of the ecliptic
  → declination      = asin(sin(obliquity) · sin(apparentLongitude))
  → equationOfTime   (minutes)

trueSolarMinutes = utcMinutes + equationOfTime + 4 · longitude
hourAngle        = trueSolarMinutes / 4 − 180°
```

- Declination and the equation of time are worked out from the actual moment (date and time of day),
  so they change smoothly through the day, allow for leap years and drift correctly from year to year.
- In the years this app works with, sunrise and sunset come out within about a minute of published times.

### How much the two differ

The main difference is the equation of time:

| Around | Real sun compared with the original calculation |
|---|---|
| Mid-February | about 14 min later |
| Mid-April, mid-June, early September, late December | about the same |
| Early November | about 16 min earlier (the original had the sun about 16 min late) |

On top of that, the declination error adds a few minutes at some times of year.

## Sun events (dawn, sunrise, sunset, dusk)

Only the current code has these. `SolarCalculator.FindSunTimes` finds them for one local day:

| Event | Sun elevation | Constant |
|---|---|---|
| Dawn / dusk (civil twilight) | −6° | `CivilTwilightElevation` |
| Sunrise / sunset | −0.833° (the sun's upper edge, atmospheric bending included) | `SunriseElevation` |

How the search works:

- It samples the elevation every 5 minutes from local midnight. Each event is the **first upward**
  or **last downward** crossing of its elevation, refined by 20 bisection steps (well under a second).
- It uses the same elevation function as the light, through `SunIRL.SolarPosition`. This keeps the
  times in step with the lighting and makes them correct on days when summer time starts or ends.
- An event that doesn't happen that day is `null`. This covers polar day, polar night and white
  nights, where the sun never gets down to −6°.

Reached through `SunIRL.GetSunTimes(date)` and `SunManager.GetSunTimes(date)`. The UI stores the
result in `SunIRLUI.SelectedDaySunTimes`.

## Verification

Prague (50.08° N, 14.42° E), local time. Each result was compared with published times and was
within a minute:

| Date | Dawn | Sunrise | Sunset | Dusk |
|---|---|---|---|---|
| 20 March | 05:33 | 06:05 | 18:15 | 18:47 |
| 29 March (first day of summer time) | 06:13 | 06:45 | 19:29 | 20:02 |
| 21 June | 04:07 | 04:52 | 21:15 | 22:00 |
| 25 October (last day of summer time) | 06:06 | 06:40 | 16:51 | 17:24 |
| 21 December | 07:19 | 07:58 | 16:02 | 16:40 |

Tromsø, in the Arctic: on 21 June there are no events (polar day). On 21 December there is only
civil twilight, from dawn 09:31 to dusk 13:53, with no sunrise.

## Reference

NOAA Global Monitoring Laboratory, Solar Calculator, and the spreadsheet formulas behind it,
which are based on Jean Meeus, *Astronomical Algorithms*.
