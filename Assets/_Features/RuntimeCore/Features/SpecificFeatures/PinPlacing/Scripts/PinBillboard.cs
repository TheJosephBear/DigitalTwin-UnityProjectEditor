using System.Collections.Generic;
using TMPro;
using UnityEngine;

/// <summary>
/// Pin visual: a camera facing head on a leg, drawn on top of everything (PinOverlay shader).
/// The head and the leg's width keep the same size on screen within ConstantSizeDistance. The leg is a real segment along the pin's up,
/// so it gets shorter when looked at from above or below. Behind geometry the pin is faded and can't be clicked.
/// Sits on a child of the pin, the leg starts at the pin's position.
/// </summary>
public class PinBillboard : MonoBehaviour {

    [Header("Parts")]
    [SerializeField] MeshRenderer _head;
    [SerializeField] MeshRenderer _leg;
    [SerializeField] TextMeshPro _label;

    [Header("Size (fractions of the screen height)")]
    public float HeadScreenSize = 0.04f;
    [Tooltip("Leg length on screen when looked at from the side")]
    public float LegScreenLength = 0.07f;
    public float LegScreenWidth = 0.006f;
    [Tooltip("Quad size / head size, room for the selection glow")]
    public float HeadPadding = 1.8f;
    public float HoverScale = 1.2f;
    [Tooltip("[min, max] camera distance in which the pin keeps its size on screen. Closer than min it stops shrinking (grows on screen), farther than max it stops growing (shrinks on screen)")]
    public Vector2 ConstantSizeDistance = new Vector2(2f, 300f);
    [Tooltip("Farther than ConstantSizeDistance max the pin shrinks on screen, but never below this fraction of its normal screen size")]
    [Range(0f, 1f)] public float MinScreenScale = 0.5f;

    [Header("Colors")]
    public Color Purple = new Color32(91, 74, 217, 255);
    public Color White = Color.white;
    [Tooltip("Outline width, fraction of the head radius")]
    [Range(0f, 1f)] public float HeadOutline = 0.16f;
    [Tooltip("Outline width, fraction of the leg's half width")]
    [Range(0f, 1f)] public float LegOutline = 0.3f;
    [Range(0f, 1f)] public float SelectedGlow = 0.8f;

    [Header("Occlusion")]
    [Tooltip("Alpha while something is between the camera and the pin")]
    [Range(0f, 1f)] public float OccludedAlpha = 0.45f;
    [Tooltip("Geometry that hides pins")]
    public LayerMask OcclusionLayers = ~0;

    static readonly RaycastHit[] _hits = new RaycastHit[32];
    static readonly List<PinBillboard> _active = new List<PinBillboard>();
    static int _sortedFrame = -1;

    static readonly int FillColorId = Shader.PropertyToID("_FillColor");
    static readonly int OutlineColorId = Shader.PropertyToID("_OutlineColor");
    static readonly int GlowColorId = Shader.PropertyToID("_GlowColor");
    static readonly int CircleId = Shader.PropertyToID("_Circle");
    static readonly int PaddingId = Shader.PropertyToID("_Padding");
    static readonly int OutlineId = Shader.PropertyToID("_Outline");
    static readonly int GlowId = Shader.PropertyToID("_Glow");
    static readonly int AlphaId = Shader.PropertyToID("_Alpha");

    MaterialPropertyBlock _block;
    bool _selected;
    bool _hovered;
    bool _occluded;
    float _depth;

    public bool Selected { get => _selected; set { _selected = value; ApplyStyle(); } }
    public bool Hovered { get => _hovered; set => _hovered = value; }
    /// <summary>Something is between the main camera and the head (as of the last rendered frame).</summary>
    public bool Occluded => _occluded;

    /// <summary>Text inside the head, "" for none.</summary>
    public void SetLabel(string text) {
        if (_label == null) return;
        _label.text = text;
        _label.gameObject.SetActive(!string.IsNullOrEmpty(text));
    }

    void Awake() {
        _block = new MaterialPropertyBlock();
        ApplyStyle();
    }

    void OnEnable() {
        _active.Add(this);
        Camera.onPreCull += OnCameraPreCull;
    }

    void OnDisable() {
        _active.Remove(this);
        Camera.onPreCull -= OnCameraPreCull;
    }

    void OnValidate() {
        if (_block != null) ApplyStyle();
    }

    // Right before rendering, after everything (RTG, Cinemachine) has moved the camera this frame
    void OnCameraPreCull(Camera cam) {
        if (cam != Camera.main) return;
        if (_sortedFrame != Time.frameCount) SortAll(cam);

        Transform camTransform = cam.transform;
        Vector3 basePosition = transform.position;
        if (!cam.orthographic && _depth <= 0f) return; // behind the camera

        Vector3 axis = transform.up;
        Vector3 headPosition = basePosition + axis * (ViewHeight(cam, basePosition) * LegScreenLength);

        // Head
        float headSize = ViewHeight(cam, headPosition) * HeadScreenSize * (_hovered ? HoverScale : 1f);
        _head.transform.SetPositionAndRotation(headPosition, camTransform.rotation);
        _head.transform.localScale = Vector3.one * (headSize * HeadPadding);

        // Leg: a bar from the base to the head's edge (not into it, it would show through a faded head), turned around its axis to face the camera
        float legLength = Vector3.Distance(basePosition, headPosition);
        float legEnd = legLength - LegLengthInsideHead(cam, axis, headPosition, headSize * 0.5f, ViewHeight(cam, headPosition) * LegScreenWidth * 0.5f);
        Vector3 legTop = basePosition + axis * legEnd;
        Vector3 toCamera = Vector3.ProjectOnPlane(camTransform.position - (basePosition + legTop) * 0.5f, axis);
        if (cam.orthographic) toCamera = Vector3.ProjectOnPlane(-camTransform.forward, axis);
        bool legVisible = legEnd > 0f && toCamera.sqrMagnitude > 0.000001f;
        _leg.enabled = legVisible;
        if (legVisible) {
            Vector3 middle = (basePosition + legTop) * 0.5f;
            _leg.transform.SetPositionAndRotation(middle, Quaternion.LookRotation(-toCamera, axis));
            _leg.transform.localScale = new Vector3(ViewHeight(cam, middle) * LegScreenWidth, legEnd, 1f);
        }

        SetOccluded(IsOccluded(camTransform.position, headPosition));
    }

    // How much of the leg's top end lies inside the head circle on screen. The head lies in the screen plane, so a step along the axis
    // moves on screen by the part of it across the view ray. The leg's corners touch the circle instead of its middle (halfWidth)
    static float LegLengthInsideHead(Camera cam, Vector3 axis, Vector3 headPosition, float radius, float halfWidth) {
        Vector3 forward = cam.transform.forward;
        Vector3 viewRay = cam.orthographic ? forward : (headPosition - cam.transform.position).normalized;
        float rayDotForward = Mathf.Max(Vector3.Dot(viewRay, forward), 0.0001f);
        float onScreen = (axis - viewRay * (Vector3.Dot(axis, forward) / rayDotForward)).magnitude; // head plane distance per unit along the axis
        float reach = Mathf.Sqrt(Mathf.Max(radius * radius - halfWidth * halfWidth, 0f));
        return onScreen > 0.0001f ? reach / onScreen : float.MaxValue;
    }

    // World height the camera sees at the point's depth, the depth clamped to ConstantSizeDistance.
    // Past the max it's kept at least MinScreenScale of the real view height, so far pins don't shrink below that on screen
    float ViewHeight(Camera cam, Vector3 point) {
        if (cam.orthographic) return 2f * cam.orthographicSize;
        float depth = Vector3.Dot(point - cam.transform.position, cam.transform.forward);
        float clamped = Mathf.Clamp(depth, Mathf.Max(ConstantSizeDistance.x, cam.nearClipPlane), ConstantSizeDistance.y);
        float tan = Mathf.Tan(cam.fieldOfView * 0.5f * Mathf.Deg2Rad);
        return 2f * Mathf.Max(clamped, depth * MinScreenScale) * tan;
    }

    // Nearer pins draw over farther ones, each pin's leg, head and text in that order
    static void SortAll(Camera cam) {
        _sortedFrame = Time.frameCount;
        Transform camTransform = cam.transform;
        foreach (PinBillboard pin in _active) {
            pin._depth = Vector3.Dot(pin.transform.position - camTransform.position, camTransform.forward);
        }
        _active.Sort((a, b) => b._depth.CompareTo(a._depth));

        for (int i = 0; i < _active.Count; i++) {
            int order = 100 + i * 3;
            PinBillboard pin = _active[i];
            pin._leg.sortingOrder = order;
            pin._head.sortingOrder = order + 1;
            if (pin._label != null) pin._label.sortingOrder = order + 2;
        }
    }

    // Anything but pins between the camera and the head
    bool IsOccluded(Vector3 camPosition, Vector3 headPosition) {
        Vector3 toHead = headPosition - camPosition;
        int count = Physics.RaycastNonAlloc(camPosition, toHead.normalized, _hits, toHead.magnitude, OcclusionLayers, QueryTriggerInteraction.Ignore);
        for (int i = 0; i < count; i++) {
            if (_hits[i].collider.GetComponentInParent<PinObject>() == null) return true;
        }
        return false;
    }

    void SetOccluded(bool occluded) {
        if (occluded == _occluded) return;
        _occluded = occluded;
        ApplyStyle();
    }

    // Normal: purple with a white outline and white text. Selected: white with a purple outline, purple text and a glow
    void ApplyStyle() {
        if (_head == null || _leg == null) return;
        float alpha = _occluded ? OccludedAlpha : 1f;

        _block.Clear();
        _block.SetColor(FillColorId, _selected ? White : Purple);
        _block.SetColor(OutlineColorId, _selected ? Purple : White);
        _block.SetColor(GlowColorId, Purple);
        _block.SetFloat(CircleId, 1f);
        _block.SetFloat(PaddingId, HeadPadding);
        _block.SetFloat(OutlineId, HeadOutline);
        _block.SetFloat(GlowId, _selected ? SelectedGlow : 0f);
        _block.SetFloat(AlphaId, alpha);
        _head.SetPropertyBlock(_block);

        _block.Clear();
        _block.SetColor(FillColorId, Purple);
        _block.SetColor(OutlineColorId, White);
        _block.SetFloat(CircleId, 0f);
        _block.SetFloat(PaddingId, 1f);
        _block.SetFloat(OutlineId, LegOutline);
        _block.SetFloat(GlowId, 0f);
        _block.SetFloat(AlphaId, alpha);
        _leg.SetPropertyBlock(_block);

        if (_label != null) {
            Color text = _selected ? Purple : White;
            text.a = alpha;
            _label.color = text;
        }
    }
}
