using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Colours for the time slider's track: the sky colour at the sun's elevation through one day,
/// so night, dawn, sunrise, day, sunset and dusk sit where they really are on that date and place.
/// </summary>
public static class DaylightTrackColors {
    private const int SampleMinutes = 10;

    // Sky colour by sun elevation in degrees (the same in the morning and the evening)
    private static readonly (float Elevation, Color Color)[] Ramp = {
        (-18f, Hex(0x16122E)),                                  // night
        (-12f, Hex(0x221C4A)),
        ((float)SolarCalculator.CivilTwilightElevation, Hex(0x3D3478)), // dawn / dusk
        (-3f, Hex(0x9A5A8E)),
        ((float)SolarCalculator.SunriseElevation, Hex(0xF0A047)),       // sunrise / sunset
        (3f, Hex(0xF6C76A)),
        (8f, Hex(0xC4E3F3)),
        (25f, Hex(0xA8D8F2)),
        (50f, Hex(0x8FCBEE)),                                   // high summer sun
    };

    /// <summary>
    /// Stops for a track that runs from local midnight (0) to spanMinutes later (1): the sky sampled
    /// every 10 minutes, plus a stop exactly at each of the day's dawn, sunrise, sunset and dusk.
    /// </summary>
    public static List<GradientPillTrack.ColorStop> Build(Func<double, double> elevationAtMinute, SunTimes times, float spanMinutes) {
        var stops = new List<GradientPillTrack.ColorStop>();
        if (spanMinutes <= 0f) return stops;

        for (float minute = 0f; minute < spanMinutes; minute += SampleMinutes) {
            AddStop(minute);
        }
        AddStop(spanMinutes);

        AddEvent(times.Dawn);
        AddEvent(times.Sunrise);
        AddEvent(times.Sunset);
        AddEvent(times.Dusk);
        return stops;

        void AddEvent(TimeSpan? time) {
            if (time.HasValue && time.Value.TotalMinutes <= spanMinutes) {
                AddStop((float)time.Value.TotalMinutes);
            }
        }

        void AddStop(float minute) {
            Color color = AtElevation((float)elevationAtMinute(minute));
            stops.Add(new GradientPillTrack.ColorStop(minute / spanMinutes, color));
        }
    }

    public static Color AtElevation(float elevation) {
        if (elevation <= Ramp[0].Elevation) return Ramp[0].Color;
        for (int i = 1; i < Ramp.Length; i++) {
            if (elevation <= Ramp[i].Elevation) {
                float t = Mathf.InverseLerp(Ramp[i - 1].Elevation, Ramp[i].Elevation, elevation);
                return Color.Lerp(Ramp[i - 1].Color, Ramp[i].Color, t);
            }
        }
        return Ramp[Ramp.Length - 1].Color;
    }

    private static Color Hex(int rgb) {
        return new Color(((rgb >> 16) & 0xFF) / 255f, ((rgb >> 8) & 0xFF) / 255f, (rgb & 0xFF) / 255f, 1f);
    }
}
