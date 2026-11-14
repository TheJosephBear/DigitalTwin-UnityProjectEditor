using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

// Rounded (pill-shaped) slider track filled with a horizontal gradient, e.g. seasons for a date
// slider or night/day for a time slider. Drawn as a mesh with a 1 px soft edge, so it stays crisp
// at any size and needs no sprite.
[RequireComponent(typeof(CanvasRenderer))]
public class GradientPillTrack: MaskableGraphic {
    [SerializeField] private Gradient gradient = new Gradient();

    [Tooltip("Where the gradient starts and ends, measured in from both ends of the track. " +
             "Set it to the slider's handle inset so the colour under the knob matches its value.")]
    [SerializeField, Min(0f)] private float gradientInset = 0f;

    [Header("Outline")]
    [SerializeField, Min(0f)] private float outlineWidth = 1f;
    [SerializeField] private Color outlineColor = new Color(0f, 0f, 0f, 0.15f);

    [Header("Quality")]
    [SerializeField, Range(4, 64)] private int capSegments = 16;
    [Tooltip("Distance between gradient samples along the straight part, in canvas units")]
    [SerializeField, Min(1f)] private float sampleSpacing = 4f;
    [Tooltip("Width of the anti-aliased edge, in screen pixels")]
    [SerializeField, Min(0f)] private float featherPixels = 1f;

    private readonly List<Column> columns = new List<Column>();
    private readonly List<float> straightXs = new List<float>();
    private readonly List<ColorStop> colorStops = new List<ColorStop>();

    /// <summary>A colour at a position along the gradient (0 = first knob position, 1 = last).</summary>
    public struct ColorStop {
        public float Position;
        public Color Color;

        public ColorStop(float position, Color color) {
            Position = position;
            Color = color;
        }
    }

    public Gradient Gradient {
        get => gradient;
        set { gradient = value; SetVerticesDirty(); }
    }

    /// <summary>
    /// Colours the track from computed stops instead of the Gradient: any number of them, each drawn
    /// exactly at its position (a Gradient holds at most 8 keys). Stops get sorted by position.
    /// </summary>
    public void SetColorStops(IEnumerable<ColorStop> stops) {
        colorStops.Clear();
        colorStops.AddRange(stops);
        colorStops.Sort((a, b) => a.Position.CompareTo(b.Position));
        SetVerticesDirty();
    }

    /// <summary>Goes back to colouring the track with the Gradient.</summary>
    public void ClearColorStops() {
        colorStops.Clear();
        SetVerticesDirty();
    }

    private Color Evaluate(float t) {
        if (colorStops.Count == 0) return gradient.Evaluate(t);

        if (t <= colorStops[0].Position) return colorStops[0].Color;
        int last = colorStops.Count - 1;
        if (t >= colorStops[last].Position) return colorStops[last].Color;

        // First stop past t (binary search; the stops are sorted)
        int lo = 1, hi = last;
        while (lo < hi) {
            int mid = (lo + hi) / 2;
            if (colorStops[mid].Position < t) lo = mid + 1; else hi = mid;
        }
        ColorStop from = colorStops[lo - 1];
        ColorStop to = colorStops[lo];
        float span = to.Position - from.Position;
        return span > 0f ? Color.Lerp(from.Color, to.Color, (t - from.Position) / span) : to.Color;
    }

    private struct Column {
        public float X;
        public float HalfHeight;
        public Vector2 Normal; // outward normal of the top edge
    }

    protected override void OnPopulateMesh(VertexHelper vh) {
        vh.Clear();

        Rect rect = GetPixelAdjustedRect();
        if (rect.width <= 0f || rect.height <= 0f) return;

        float feather = featherPixels / (canvas != null ? Mathf.Max(canvas.scaleFactor, 0.0001f) : 1f);
        float gradientStart = rect.xMin + gradientInset;
        float gradientWidth = Mathf.Max(rect.width - 2f * gradientInset, 0.0001f);
        Color tint = color;

        if (outlineWidth > 0f && outlineColor.a > 0f) {
            Color outline = outlineColor * tint;
            AddPill(vh, rect, feather, _ => outline);
        }

        Rect inner = outlineWidth > 0f
            ? new Rect(rect.xMin + outlineWidth, rect.yMin + outlineWidth, rect.width - 2f * outlineWidth, rect.height - 2f * outlineWidth)
            : rect;
        if (inner.width <= 0f || inner.height <= 0f) return;

        AddPill(vh, inner, feather, x => Evaluate(Mathf.Clamp01((x - gradientStart) / gradientWidth)) * tint,
            t => gradientStart + t * gradientWidth);
    }

    // Triangle strip running left to right; each column has 4 vertices:
    // outer top (transparent), inner top, inner bottom, outer bottom (transparent)
    // stopToX: where a colour stop lies on the track, so each stop gets its own column and its colour lands exactly there
    private void AddPill(VertexHelper vh, Rect rect, float feather, System.Func<float, Color> colorAt, System.Func<float, float> stopToX = null) {
        float radius = Mathf.Min(rect.height, rect.width) * 0.5f;
        float centerY = rect.center.y;
        float leftCenter = rect.xMin + radius;
        float rightCenter = rect.xMax - radius;

        columns.Clear();
        for (int i = 0; i <= capSegments; i++) { // left cap: from the left end up to the top
            float angle = Mathf.PI - i * (Mathf.PI * 0.5f) / capSegments;
            AddCapColumn(leftCenter, radius, angle);
        }
        straightXs.Clear();
        int straightSamples = Mathf.CeilToInt((rightCenter - leftCenter) / sampleSpacing);
        for (int i = 1; i < straightSamples; i++) {
            straightXs.Add(Mathf.Lerp(leftCenter, rightCenter, (float)i / straightSamples));
        }
        if (stopToX != null) {
            foreach (ColorStop stop in colorStops) {
                float x = stopToX(stop.Position);
                if (x > leftCenter && x < rightCenter) straightXs.Add(x);
            }
            straightXs.Sort();
        }
        float previousX = leftCenter;
        foreach (float x in straightXs) {
            if (x - previousX < 0.01f) continue; // skip duplicates
            columns.Add(new Column { X = x, HalfHeight = radius, Normal = Vector2.up });
            previousX = x;
        }
        for (int i = 0; i <= capSegments; i++) { // right cap: from the top down to the right end
            float angle = Mathf.PI * 0.5f - i * (Mathf.PI * 0.5f) / capSegments;
            AddCapColumn(rightCenter, radius, angle);
        }

        float halfFeather = feather * 0.5f;
        int start = vh.currentVertCount;
        for (int i = 0; i < columns.Count; i++) {
            Column column = columns[i];
            Color solid = colorAt(column.X);
            Color clear = solid;
            clear.a = 0f;

            Vector2 top = new Vector2(column.X, centerY + column.HalfHeight);
            Vector2 bottom = new Vector2(column.X, centerY - column.HalfHeight);
            Vector2 normalTop = column.Normal;
            Vector2 normalBottom = new Vector2(column.Normal.x, -column.Normal.y);

            vh.AddVert(top + normalTop * halfFeather, clear, Vector4.zero);
            vh.AddVert(top - normalTop * halfFeather, solid, Vector4.zero);
            vh.AddVert(bottom - normalBottom * halfFeather, solid, Vector4.zero);
            vh.AddVert(bottom + normalBottom * halfFeather, clear, Vector4.zero);

            if (i == 0) continue;
            int a = start + (i - 1) * 4;
            int b = start + i * 4;
            for (int band = 0; band < 3; band++) {
                vh.AddTriangle(a + band, b + band, b + band + 1);
                vh.AddTriangle(b + band + 1, a + band + 1, a + band);
            }
        }
    }

    private void AddCapColumn(float centerX, float radius, float angle) {
        Vector2 normal = new Vector2(Mathf.Cos(angle), Mathf.Sin(angle));
        columns.Add(new Column {
            X = centerX + radius * normal.x,
            HalfHeight = radius * normal.y,
            Normal = normal
        });
    }
}
