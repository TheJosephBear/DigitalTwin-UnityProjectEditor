using System;
using UnityEngine;
using UnityEngine.Experimental.GlobalIllumination;

public class SunIRL: MonoBehaviour {

    [Header("References")]
    [SerializeField] private GameObject DirectionalLightReference;
    [SerializeField] private Light DirectionalLightComponent;

    [Header("Lighting Settings")]
    [SerializeField] private float MaxIntensity = 1.2f;
    [SerializeField] private float SunsetFadeAngle = 5f;

    [Header("Spatial Coordinates")]
    [Range(-90f, 90f)] public float Latitude = 0f;
    [Range(-180f, 180f)] public float Longitude = 0f;

    [Header("Timezone")]
    [Tooltip("Standard (winter) time offset from UTC in hours (e.g., +1 for CET, -5 for EST)")]
    public float StandardUtcOffsetHours = 1f;
    [Tooltip("Adds +1 h between the last Sunday of March and the last Sunday of October (EU rule, e.g. CEST)")]
    public bool UseEuDaylightSaving = true;

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
        Month = (uint)Mathf.Clamp((int)month, 1, 12);
        UpdateSunPosition();
    }

    public void SetDay(uint day) {
        Day = (uint)Mathf.Clamp((int)day, 1, 31);
        UpdateSunPosition();
    }

    public void SetHour(uint hour) {
        Hour = (uint)Mathf.Clamp((int)hour, 0, 23);
        UpdateSunPosition();
    }

    public void SetMinute(uint minute) {
        Minute = (uint)Mathf.Clamp((int)minute, 0, 59);
        UpdateSunPosition();
    }

    #endregion

    #region Time Helpers

    /// <summary>
    /// The currently set date and time; the day is clamped to the month length so e.g. 30. 2. stays valid.
    /// </summary>
    public DateTime GetDateTime() {
        try {
            int year = (int)Year;
            int month = Mathf.Clamp((int)Month, 1, 12);
            int day = Mathf.Clamp((int)Day, 1, DateTime.DaysInMonth(year, month));
            return new DateTime(year, month, day, Mathf.Clamp((int)Hour, 0, 23), Mathf.Clamp((int)Minute, 0, 59), 0, DateTimeKind.Unspecified);
        } catch {
            return new DateTime(2026, 1, 1, 12, 0, 0, DateTimeKind.Unspecified);
        }
    }

    /// <summary>
    /// Current wall-clock time in this sun's timezone. Built from UTC on purpose, so it
    /// doesn't depend on the browser's timezone (unreliable in WebGL).
    /// </summary>
    public DateTime GetCurrentLocalTime() {
        DateTime utcNow = DateTime.UtcNow;
        float offset = StandardUtcOffsetHours + (UseEuDaylightSaving && IsEuDaylightSaving(utcNow) ? 1f : 0f);
        return DateTime.SpecifyKind(utcNow.AddHours(offset), DateTimeKind.Unspecified);
    }

    /// <summary>
    /// UTC offset in effect at the given local time (standard offset, +1 h during EU summer time).
    /// </summary>
    public float GetUtcOffsetHours(DateTime localTime) {
        if (!UseEuDaylightSaving) return StandardUtcOffsetHours;

        DateTime approxUtc = localTime.AddHours(-StandardUtcOffsetHours);
        return StandardUtcOffsetHours + (IsEuDaylightSaving(approxUtc) ? 1f : 0f);
    }

    // EU summer time runs from 01:00 UTC on the last Sunday of March to 01:00 UTC on the last Sunday of October.
    // Computed by hand because TimeZoneInfo has no timezone data in WebGL builds.
    private static bool IsEuDaylightSaving(DateTime utc) {
        DateTime start = LastSunday(utc.Year, 3).AddHours(1);
        DateTime end = LastSunday(utc.Year, 10).AddHours(1);
        return utc >= start && utc < end;
    }

    private static DateTime LastSunday(int year, int month) {
        DateTime lastDay = new DateTime(year, month, DateTime.DaysInMonth(year, month));
        return lastDay.AddDays(-(int)lastDay.DayOfWeek);
    }

    #endregion

    public void UpdateSunPosition() {
        if (DirectionalLightReference == null) return;

        // Cache the Light component reference automatically
        if (DirectionalLightComponent == null) {
            DirectionalLightComponent = DirectionalLightReference.GetComponent<Light>();
        }

        Quaternion calculatedRotation = CalculateSunQuaternion(out float altitudeDeg);
        DirectionalLightReference.transform.rotation = calculatedRotation;

        // Smoothly fade light intensity near and below the horizon
        if (DirectionalLightComponent != null) {
            // Evaluate altitude: > 5 deg = 100% brightness, < 0 deg = 0% brightness
            float lightFactor = Mathf.Clamp01(altitudeDeg / SunsetFadeAngle);

            DirectionalLightComponent.intensity = Mathf.Lerp(0f, MaxIntensity, lightFactor);

            // Disable shadows below horizon to prevent unwanted subsurface light artifacts
            DirectionalLightComponent.shadows = (altitudeDeg > 0f) ? LightShadows.Soft : LightShadows.None;
        }
    }

    private Quaternion CalculateSunQuaternion(out float altitudeDeg) {
        SolarPosition(GetDateTime(), out double elevationDeg, out double azimuthDeg);
        altitudeDeg = (float)elevationDeg;

        // Azimuth is measured clockwise from true north; the light points away from the sun
        return Quaternion.Euler(altitudeDeg, (float)azimuthDeg + 180f, 0f);
    }

    // Sun position at a local wall-clock time here (summer/winter time aware)
    private void SolarPosition(DateTime localTime, out double elevationDeg, out double azimuthDeg) {
        DateTime utc = localTime.AddHours(-GetUtcOffsetHours(localTime));
        SolarCalculator.Position(utc, Latitude, Longitude, out elevationDeg, out azimuthDeg);
    }

    /// <summary>
    /// Sun elevation in degrees at a local wall-clock time, at this sun's location.
    /// </summary>
    public float GetElevation(DateTime localTime) {
        SolarPosition(localTime, out double elevationDeg, out _);
        return (float)elevationDeg;
    }

    /// <summary>
    /// Dawn, sunrise, sunset and dusk on the given local date, at this sun's location.
    /// </summary>
    public SunTimes GetSunTimes(DateTime date) {
        DateTime midnight = date.Date;
        return SolarCalculator.FindSunTimes(minutes => {
            SolarPosition(midnight.AddMinutes(minutes), out double elevationDeg, out _);
            return elevationDeg;
        });
    }

    public SerializableSun Serialize() {
        return new SerializableSun {
            year = (int)Year,
            month = (int)Month,
            day = (int)Day,
            hour = (int)Hour,
            minute = (int)Minute,
        };
    }

    public void Deserialize(SerializableSun serializedSun, Vector2 geoCoordinates) {
        if (serializedSun == null) return;

        SetYear((uint)serializedSun.year);
        SetMonth((uint)serializedSun.month);
        SetDay((uint)serializedSun.day);
        SetHour((uint)serializedSun.hour);
        SetMinute((uint)serializedSun.minute);

        // Also 0, 0 for a project not placed on the map, so it doesn't keep the previous project's location
        SetCoordinates(geoCoordinates.x, geoCoordinates.y);
    }
}
