using System.Collections.Generic;
using UnityEngine;

public class PinObject: EditorObjectBase, IClickableBehindGeometry {
    [Header("Pin Data")]
    public Sprite Image;

    [Header("Outline")]
    [Tooltip("Added to the model's renderers while the pin is being edited (PinOutline shader)")]
    [SerializeField] private Material _outlineMaterial;
    [Tooltip("Enlarged on hover")]
    [SerializeField] private GameObject _headObject;

    [Header("Camera")]
    [Tooltip("Vcam the camera flies to when the pin is clicked. Child in the prefab, detached on Awake so the pin's scaling doesn't move it")]
    [SerializeField] private GameObject _vcam;

    [Header("Hover Settings")]
    [SerializeField] private Vector3 hoverScale = new Vector3(1.2f, 1.2f, 1.2f);
    private Vector3 originalScale;
    private bool _placing;
    private Renderer[] _renderers;
    private bool _outlined;

    const float VcamClearance = 0.3f; // How far the vcam stays from walls

    // Tried in order when the preferred vcam direction is blocked
    static readonly float[] VcamPitchOffsets = { 0f, 15f, -15f, 30f };
    static readonly float[] VcamYawOffsets = { 0f, 45f, -45f, 90f, -90f, 135f, -135f, 180f };

    public GameObject Vcam => _vcam;

    Vector3 _surfaceNormal = Vector3.up;

    /// <summary>
    /// Normal of the surface the pin was placed on. Setting it turns the pin: upright on floors,
    /// leaning out of walls so the leg's tip touches them.
    /// </summary>
    public Vector3 SurfaceNormal {
        get => _surfaceNormal;
        set {
            _surfaceNormal = value.sqrMagnitude > 0.0001f ? value.normalized : Vector3.up;
            transform.rotation = Mathf.Abs(_surfaceNormal.y) < 0.3f
                ? Quaternion.LookRotation(-_surfaceNormal, Vector3.up) * Quaternion.Euler(-15f, 0f, 0f)
                : Quaternion.FromToRotation(Vector3.up, _surfaceNormal);
        }
    }

    /// <summary>ImageManager IDs of the pin's images, in the order they are shown.</summary>
    public List<string> ImageIds { get; private set; } = new List<string>();

    /// <summary>Description of each image, same order and count as ImageIds ("" when there is none).</summary>
    public List<string> ImageDescriptions { get; private set; } = new List<string>();

    public void SetImages(List<string> ids, List<string> descriptions) {
        ImageIds = ids != null ? new List<string>(ids) : new List<string>();
        ImageDescriptions = descriptions != null ? new List<string>(descriptions) : new List<string>();

        // Older saves have no descriptions
        while (ImageDescriptions.Count < ImageIds.Count) ImageDescriptions.Add("");
        if (ImageDescriptions.Count > ImageIds.Count) ImageDescriptions.RemoveRange(ImageIds.Count, ImageDescriptions.Count - ImageIds.Count);
    }

    private void Awake() {
        originalScale = _headObject != null ? _headObject.transform.localScale : Vector3.one;
        _renderers = GetComponentsInChildren<Renderer>(true);

        if (_vcam != null) {
            _vcam.transform.SetParent(null, true); // stays in the pin's scene
            _vcam.SetActive(false);
        }
    }

    private void OnDestroy() {
        if (_vcam != null) Destroy(_vcam);
    }

    // Pins only follow the model while it's being geolocated, they can't be opened then
    static bool IsInteractionBlocked => EditorManager.Instance.ActiveState == AppState.Survey || EditorManager.Instance.ActiveState == AppState.GeoLocalization;

    public void OnClickDown() {
        if (PinManager.Instance.IsPlacing) return;
        if (IsInteractionBlocked) return;
        PinManager.Instance.SelectPin(this);
        PinManager.Instance.FocusPin(this);
    }

    public void OnClick() { }
    public void OnClickUp() { }

    public void OnHover() {
        if (IsInteractionBlocked) return;
        if (_headObject != null) _headObject.transform.localScale = Vector3.Scale(originalScale, hoverScale);
    }

    public void OnUnhover() {
        if (_headObject != null) _headObject.transform.localScale = originalScale;
    }

    public void SetOutline(bool toggleOn) {
        if (_outlineMaterial == null || toggleOn == _outlined) return;
        _outlined = toggleOn;

        foreach (Renderer renderer in _renderers) {
            List<Material> materials = new List<Material>(renderer.sharedMaterials);
            if (toggleOn) materials.Add(_outlineMaterial);
            else materials.Remove(_outlineMaterial);
            renderer.sharedMaterials = materials.ToArray();
        }
    }

    /// <summary>
    /// While placing, the pin follows the cursor, so its colliders must not block the placing raycast.
    /// </summary>
    public void SetPlacingMode(bool placing) {
        _placing = placing;
        foreach (Collider col in GetComponentsInChildren<Collider>()) {
            col.enabled = !_placing;
        }
        if (placing) OnUnhover();
    }

    #region Vcam

    /// <summary>
    /// Puts the vcam the given distance from the pin, looking down at it at the given pitch.
    /// Faces the pin from the side its surface faces (from the main camera's side on floors).
    /// When geometry is in the way it tries other directions, and never goes past a wall.
    /// </summary>
    public void PlaceVcam(float distance, float pitch, LayerMask obstacles) {
        if (_vcam == null) return;

        Vector3 target = transform.position;
        Camera cam = Camera.main;
        // Casts start off the surface, so they don't hit the surface the pin is on
        Vector3 origin = target + SurfaceNormal * (VcamClearance + 0.05f);

        // Horizontal direction from the pin toward where the vcam should be
        Vector3 heading = Vector3.ProjectOnPlane(SurfaceNormal, Vector3.up);
        if (heading.sqrMagnitude < 0.01f && cam != null) heading = Vector3.ProjectOnPlane(cam.transform.position - target, Vector3.up);
        if (heading.sqrMagnitude < 0.0001f) heading = Vector3.back;
        heading.Normalize();

        Vector3 bestDirection = Vector3.up;
        float bestFree = -1f;
        bool found = false;
        for (int p = 0; p < VcamPitchOffsets.Length && !found; p++) {
            float elevation = Mathf.Clamp(pitch + VcamPitchOffsets[p], 5f, 85f);
            for (int y = 0; y < VcamYawOffsets.Length && !found; y++) {
                Vector3 horizontal = Quaternion.AngleAxis(VcamYawOffsets[y], Vector3.up) * heading;
                Vector3 direction = Quaternion.AngleAxis(elevation, Vector3.Cross(horizontal, Vector3.up)) * horizontal;
                found = TryDirection(direction);
            }
        }

        // Everything is blocked somewhere, the main camera is looking at the pin so its direction is likely clear
        if (!found && cam != null) TryDirection((cam.transform.position - origin).normalized);

        Vector3 position = origin + bestDirection * bestFree;
        Vector3 forward = target - position;
        if (forward.sqrMagnitude < 0.0001f) forward = -bestDirection;
        Vector3 up = Vector3.Cross(forward, Vector3.up).sqrMagnitude < 0.0001f ? heading : Vector3.up;
        _vcam.transform.SetPositionAndRotation(position, Quaternion.LookRotation(forward, up));

        // True when the whole distance is free, otherwise remembers the direction if it's the most open one so far
        bool TryDirection(Vector3 direction) {
            float free = FreeDistance(origin, direction, distance, obstacles);
            if (free > bestFree) {
                bestFree = free;
                bestDirection = direction;
            }
            return free >= distance;
        }
    }

    /// <summary>
    /// Puts the vcam where the main camera is, looking at the pin, so flying to it only turns the camera.
    /// </summary>
    public void AimVcamFromCamera() {
        Camera cam = Camera.main;
        if (_vcam == null || cam == null) return;

        Vector3 position = cam.transform.position;
        Vector3 forward = transform.position - position;
        if (forward.sqrMagnitude < 0.0001f) forward = cam.transform.forward;
        _vcam.transform.SetPositionAndRotation(position, Quaternion.LookRotation(forward, Vector3.up));
    }

    // How far the vcam can go from the origin in the direction before hitting geometry
    float FreeDistance(Vector3 origin, Vector3 direction, float distance, LayerMask obstacles) {
        // A sphere cast doesn't see colliders it starts inside of
        foreach (Collider col in Physics.OverlapSphere(origin, VcamClearance, obstacles, QueryTriggerInteraction.Ignore)) {
            if (col.GetComponentInParent<PinObject>() == null) return 0f;
        }

        float free = distance;
        RaycastHit[] hits = Physics.SphereCastAll(origin, VcamClearance, direction, distance, obstacles, QueryTriggerInteraction.Ignore);
        foreach (RaycastHit hit in hits) {
            if (hit.collider.GetComponentInParent<PinObject>() != null) continue; // pins aren't walls
            free = Mathf.Min(free, hit.distance);
        }
        return free;
    }

    #endregion

    public SerializablePin Serialize() {
        return new SerializablePin {
            ID = ID,
            Name = Name,
            Description = Description,
            position = transform.position,
            normal = SurfaceNormal,
            imageIds = new List<string>(ImageIds),
            imageDescriptions = new List<string>(ImageDescriptions)
        };
    }

    public void Deserialize(SerializablePin data) {
        ID = data.ID;
        SetName(data.Name);
        SetDescription(data.Description);
        transform.position = data.position;
        // Older saves have no normal
        SurfaceNormal = data.normal != Vector3.zero ? data.normal : Vector3.up;
        SetImages(data.imageIds, data.imageDescriptions); // null in older saves
    }
}
