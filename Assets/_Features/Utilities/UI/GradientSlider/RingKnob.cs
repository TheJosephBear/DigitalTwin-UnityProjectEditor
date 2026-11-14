using UnityEngine;
using UnityEngine.UI;

// Round slider knob: a light disc with a coloured ring and a soft drop shadow.
// Drawn as a mesh with anti-aliased edges, sized by its RectTransform (the smaller side).
[RequireComponent(typeof(CanvasRenderer))]
public class RingKnob: MaskableGraphic {
    [SerializeField] private Color ringColor = new Color(0.416f, 0.345f, 0.655f, 1f);
    [SerializeField] private Color fillColor = Color.white;
    [SerializeField, Min(0f)] private float ringWidth = 3.5f;

    [Header("Shadow")]
    [SerializeField] private Color shadowColor = new Color(0f, 0f, 0f, 0.28f);
    [SerializeField] private Vector2 shadowOffset = new Vector2(0f, -1f);
    [SerializeField, Min(0f)] private float shadowBlur = 3f;

    [Header("Quality")]
    [SerializeField, Range(12, 128)] private int segments = 48;
    [Tooltip("Width of the anti-aliased edge, in screen pixels")]
    [SerializeField, Min(0f)] private float featherPixels = 1f;

    public Color RingColor {
        get => ringColor;
        set { ringColor = value; SetVerticesDirty(); }
    }

    protected override void OnPopulateMesh(VertexHelper vh) {
        vh.Clear();

        Rect rect = GetPixelAdjustedRect();
        float radius = Mathf.Min(rect.width, rect.height) * 0.5f;
        if (radius <= 0f) return;

        float feather = featherPixels / (canvas != null ? Mathf.Max(canvas.scaleFactor, 0.0001f) : 1f);
        Vector2 center = rect.center;
        Color tint = color;

        if (shadowColor.a > 0f) {
            AddDisc(vh, center + shadowOffset, radius, Mathf.Max(shadowBlur, feather), shadowColor * tint);
        }
        AddDisc(vh, center, radius, feather, ringColor * tint);
        if (ringWidth < radius) {
            AddDisc(vh, center, radius - ringWidth, feather, fillColor * tint);
        }
    }

    // Fan for the solid part plus a transparent band around it for the soft edge
    private void AddDisc(VertexHelper vh, Vector2 center, float radius, float edge, Color solid) {
        Color clear = solid;
        clear.a = 0f;
        float inner = Mathf.Max(radius - edge * 0.5f, 0f);
        float outer = radius + edge * 0.5f;

        int start = vh.currentVertCount;
        vh.AddVert(center, solid, Vector4.zero);
        for (int i = 0; i < segments; i++) {
            float angle = i * Mathf.PI * 2f / segments;
            Vector2 direction = new Vector2(Mathf.Cos(angle), Mathf.Sin(angle));
            vh.AddVert(center + direction * inner, solid, Vector4.zero);
            vh.AddVert(center + direction * outer, clear, Vector4.zero);
        }

        for (int i = 0; i < segments; i++) {
            int innerA = start + 1 + i * 2;
            int outerA = innerA + 1;
            int innerB = start + 1 + (i + 1) % segments * 2;
            int outerB = innerB + 1;
            vh.AddTriangle(start, innerA, innerB);
            vh.AddTriangle(innerA, outerA, outerB);
            vh.AddTriangle(outerB, innerB, innerA);
        }
    }
}
