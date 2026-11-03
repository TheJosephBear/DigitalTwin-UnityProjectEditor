using System.Collections.Generic;
using UnityEngine;

public class PinObject: EditorObjectBase, IClickable {
    [Header("Pin Data")]
    public Sprite Image;

    [Header("Outline")]
    [SerializeField] private GameObject _outlineObject; // Shown while the pin is being edited
    [SerializeField] private GameObject _headObject; // Shown while the pin is being edited

    [Header("Camera")]
    [Tooltip("Vcam the camera flies to when the pin is clicked. Child in the prefab, detached on Awake so the pin's hover scaling doesn't move it")]
    [SerializeField] private GameObject _vcam;

    [Header("Hover Settings")]
    [SerializeField] private Vector3 hoverScale = new Vector3(1.2f, 1.2f, 1.2f);
    private Vector3 originalScale;
    private bool _placing;
    private bool _occluded;

    const float VcamClearance = 0.3f; // How far the vcam stays from walls

    public GameObject Vcam => _vcam;

    /// <summary>Normal of the surface the pin was placed on, the vcam looks at the pin along it.</summary>
    public Vector3 SurfaceNormal { get; set; } = Vector3.up;

    /// <summary>ImageManager IDs of the pin's images, in the order they are shown.</summary>
    public List<string> ImageIds { get; private set; } = new List<string>();

    public void SetImageIds(List<string> ids) {
        ImageIds = ids != null ? new List<string>(ids) : new List<string>();
    }

    private void Awake() {
        originalScale = transform.localScale;

        if (_vcam != null) {
            _vcam.transform.SetParent(null, true); // stays in the pin's scene
            _vcam.SetActive(false);
        }
    }

    private void OnDestroy() {
        if (_vcam != null) Destroy(_vcam);
    }

    public void OnClickDown() {
        if (PinManager.Instance.IsPlacing) return;
        if (EditorManager.Instance.ActiveState == AppState.Survey) return;
        PinManager.Instance.SelectPin(this);
        PinManager.Instance.FocusPin(this);
    }

    public void OnClick() { }
    public void OnClickUp() { }

    public void OnHover() {
        if (EditorManager.Instance.ActiveState == AppState.Survey) return;
        _headObject.transform.localScale = Vector3.Scale(originalScale, hoverScale);
    }

    public void OnUnhover() {
        _headObject.transform.localScale = originalScale;
    }

    public void SetOutline(bool toggleOn) {
        if (_outlineObject != null) _outlineObject.SetActive(toggleOn);
    }

    /// <summary>
    /// While placing, the pin follows the cursor, so its colliders must not block the placing raycast.
    /// </summary>
    public void SetPlacingMode(bool placing) {
        _placing = placing;
        UpdateColliders();
        if (placing) OnUnhover();
    }

    /// <summary>
    /// Behind geometry the pin is still drawn, but can't be clicked (PinBillboard).
    /// </summary>
    public void SetOccluded(bool occluded) {
        _occluded = occluded;
        UpdateColliders();
    }

    void UpdateColliders() {
        foreach (Collider col in GetComponentsInChildren<Collider>()) {
            col.enabled = !_placing && !_occluded;
        }
    }

    #region Vcam

    /// <summary>
    /// Puts the vcam the given distance from the pin, perpendicular to its surface, looking at the pin.
    /// It stops short of any geometry in the way, so it never ends up inside a wall.
    /// </summary>
    public void PlaceVcam(float distance, LayerMask obstacles) {
        if (_vcam == null) return;

        Vector3 target = transform.position;
        Vector3 direction = SurfaceNormal.sqrMagnitude > 0.0001f ? SurfaceNormal.normalized : Vector3.up;
        float free = FreeDistance(target, direction, distance, obstacles);

        // Blocked close to the pin (overhang, ceiling, corner): the main camera is looking at the pin, so its direction is likely clear
        Camera cam = Camera.main;
        if (free < distance * 0.5f && cam != null) {
            Vector3 toCam = (cam.transform.position - target).normalized;
            float freeToCam = FreeDistance(target, toCam, distance, obstacles);
            if (freeToCam > free) {
                direction = toCam;
                free = freeToCam;
            }
        }

        Vector3 position = target + direction * free;

        // Looking straight down (pin on a floor): the top of the view is where the main camera was facing
        Vector3 up = Vector3.up;
        if (Vector3.Cross(direction, up).sqrMagnitude < 0.0001f) {
            up = cam != null ? Vector3.ProjectOnPlane(cam.transform.forward, Vector3.up) : Vector3.forward;
            if (up.sqrMagnitude < 0.0001f) up = Vector3.forward;
        }

        _vcam.transform.SetPositionAndRotation(position, Quaternion.LookRotation(-direction, up));
    }

    // How far the vcam can go from the pin in the direction before hitting geometry
    float FreeDistance(Vector3 from, Vector3 direction, float distance, LayerMask obstacles) {
        // Starts off the surface so the cast doesn't hit the surface the pin is on
        float start = VcamClearance + 0.05f;
        if (distance <= start) return distance;

        float free = distance;
        RaycastHit[] hits = Physics.SphereCastAll(from + direction * start, VcamClearance, direction, distance - start, obstacles, QueryTriggerInteraction.Ignore);
        foreach (RaycastHit hit in hits) {
            if (hit.collider.GetComponentInParent<PinObject>() != null) continue; // pins aren't walls
            free = Mathf.Min(free, start + hit.distance);
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
            imageIds = new List<string>(ImageIds)
        };
    }

    public void Deserialize(SerializablePin data) {
        ID = data.ID;
        SetName(data.Name);
        SetDescription(data.Description);
        transform.position = data.position;
        // Older saves have no normal
        SurfaceNormal = data.normal != Vector3.zero ? data.normal : Vector3.up;
        SetImageIds(data.imageIds); // null in older saves
    }
}
