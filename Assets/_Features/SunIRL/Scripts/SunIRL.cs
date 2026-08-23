using System;
using UnityEngine;

public class SunIRL: MonoBehaviour {

    [Header("References")]
    [SerializeField] private GameObject DirectionalLightReference;

    [Header("Spatial Coordinates")]
    [Range(-90f, 90f)] public float Latitude = 0f;
    [Range(-180f, 180f)] public float Longitude = 0f;

    [Header("Timezone")]
    [Tooltip("Local time offset from UTC in hours (e.g., +2 for CEST, -5 for EST)")]
    public float UtcOffsetHours = 0f;

    [Header("Time")]
    [SerializeField] private uint Year = 2026;
    [Range(1, 12)]
    [SerializeField] private uint Month = 1;
    [Range(1, 31)]
    [SerializeField] private uint Day = 1;
    [Range(0, 23)]
    [SerializeField] private uint Hour = 12;
    [Range(0, 59)]
    [SerializeField] private uint Minute = 0;

    private void OnValidate() {
        // Automatically updates light in Editor when inspector fields change
        UpdateSunPosition();
    }

    private void Start() {
        UpdateSunPosition();
    }

    #region Setters

    public void SetCoordinates(float latitude, float longitude) {
        Latitude = Mathf.Clamp(latitude, -90f, 90f);
        Longitude = Mathf.Clamp(longitude, -180f, 180f);
        UpdateSunPosition();
    }

    public void SetDate(DateTime dateTime) {
        Year = (uint)dateTime.Year;
        Month = (uint)dateTime.Month;
        Day = (uint)dateTime.Day;
        Hour = (uint)dateTime.Hour;
        Minute = (uint)dateTime.Minute;
        UpdateSunPosition();
    }

    public void SetYear(uint year) {
        Year = year;
        UpdateSunPosition();
    }

    public void SetMonth(uint month) {
        Month = Mathf.Clamp((int)month, 1, 12) > 0 ? month : 1;
        UpdateSunPosition();
    }

    public void SetDay(uint day) {
        Day = Mathf.Clamp((int)day, 1, 31) > 0 ? day : 1;
        UpdateSunPosition();
    }

    public void SetHour(uint hour) {
        Hour = Mathf.Clamp((int)hour, 0, 23) > 0 ? hour : 0;
        UpdateSunPosition();
    }

    public void SetMinute(uint minute) {
        Minute = Mathf.Clamp((int)minute, 0, 59) > 0 ? minute : 0;
        UpdateSunPosition();
    }

    #endregion

    public void UpdateSunPosition() {
        if (DirectionalLightReference == null) return;

        Quaternion calculatedValue = CalculateSunQuaternion();
        DirectionalLightReference.transform.rotation = calculatedValue;
    }

    private Quaternion CalculateSunQuaternion() {
        DateTime currentDateTime;
        try {
            currentDateTime = new DateTime((int)Year, (int)Month, (int)Day, (int)Hour, (int)Minute, 0, DateTimeKind.Unspecified);
        } catch {
            currentDateTime = new DateTime(2026, 1, 1, 12, 0, 0, DateTimeKind.Unspecified);
        }

        // 1. Calculate Day of the Year
        int dayOfYear = currentDateTime.DayOfYear;

        // 2. Convert Local Time to UTC Hours
        float localHours = (float)currentDateTime.TimeOfDay.TotalHours;
        float utcHours = localHours - UtcOffsetHours;

        // 3. Convert UTC to Local Solar Time using Longitude (15° per hour = 4 min per degree)
        float solarTimeHours = utcHours + (Longitude / 15f);

        // Normalize solar time between 0 and 24
        solarTimeHours = (solarTimeHours % 24f + 24f) % 24f;

        // 4. Calculate Solar Declination (radians)
        float declinationRad = 23.45f * Mathf.Deg2Rad * Mathf.Sin(Mathf.Deg2Rad * (360f / 365f * (dayOfYear + 284)));

        // 5. Calculate Hour Angle (radians)
        // -12h = Morning (-180°), 0h = Solar Noon (0°), +12h = Evening (+180°)
        float hourAngleRad = (solarTimeHours - 12f) * 15f * Mathf.Deg2Rad;

        // 6. Latitude in radians
        float latRad = Latitude * Mathf.Deg2Rad;

        // 7. Calculate Solar Altitude / Elevation angle
        float sinAltitude = Mathf.Sin(latRad) * Mathf.Sin(declinationRad) +
                            Mathf.Cos(latRad) * Mathf.Cos(declinationRad) * Mathf.Cos(hourAngleRad);
        float altitudeRad = Mathf.Asin(Mathf.Clamp(sinAltitude, -1f, 1f));

        // 8. Calculate Solar Azimuth angle (Measured from True North = 0°)
        float cosAzimuth = (Mathf.Sin(declinationRad) * Mathf.Cos(latRad) -
                            Mathf.Cos(declinationRad) * Mathf.Sin(latRad) * Mathf.Cos(hourAngleRad)) / Mathf.Max(0.0001f, Mathf.Cos(altitudeRad));

        float azimuthRad = Mathf.Acos(Mathf.Clamp(cosAzimuth, -1f, 1f));

        // Morning sun is in the East (< 180°), Afternoon sun is in the West (> 180°)
        if (hourAngleRad > 0) {
            azimuthRad = (2f * Mathf.PI) - azimuthRad;
        }

        float altitudeDeg = altitudeRad * Mathf.Rad2Deg;
        float azimuthDeg = azimuthRad * Mathf.Rad2Deg;

        float adjustedAzimuthDeg = azimuthDeg + 180f;

        return Quaternion.Euler(altitudeDeg, adjustedAzimuthDeg, 0f);
    }
}
