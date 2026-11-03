using System.Collections.Generic;
using System.Linq;
using UnityEngine;

/// <summary>
/// Pin visual: faces the main camera and keeps the same size on screen from any distance.
/// Sits on a child of the pin, the pin's own scale (hover) still enlarges it.
/// Drawn on top of everything (PinBillboard shader): behind geometry it gets a toned down color and can't be clicked.
/// </summary>
public class PinBillboard : MonoBehaviour {

    [Tooltip("Size on screen as a fraction of the screen height")]
    public float ScreenSize = 0.035f;

    [Tooltip("Color while something is between the camera and the pin")]
    public Color OccludedColor = new Color(0.6f, 0.45f, 0.45f, 0.45f);
    [Tooltip("Geometry that hides pins")]
    public LayerMask OcclusionLayers = ~0;

    static readonly RaycastHit[] _hits = new RaycastHit[32];

    List<SpriteRenderer> _renderers;
    Color _visibleColor;
    PinObject _pin;
    bool _occluded;

    void Awake() {
        _renderers = GetComponentsInChildren<SpriteRenderer>().ToList();
        _renderers.Add(GetComponent<SpriteRenderer>());
        _visibleColor = _renderers[0].color;
        _pin = GetComponentInParent<PinObject>();
    }

    void OnEnable() {
        Camera.onPreCull += OnCameraPreCull;
    }

    void OnDisable() {
        Camera.onPreCull -= OnCameraPreCull;
    }

    // Right before rendering, after everything (RTG, Cinemachine) has moved the camera this frame
    void OnCameraPreCull(Camera cam) {
        if (cam != Camera.main) return;

        Transform pin = transform.parent != null ? transform.parent : transform;
        Transform camTransform = cam.transform;

        // Perspective size on screen depends on the depth along the view direction, not the distance
        float depth = Vector3.Dot(pin.position - camTransform.position, camTransform.forward);
        if (!cam.orthographic && depth <= 0f) return;

        float viewHeight = cam.orthographic
            ? 2f * cam.orthographicSize
            : 2f * depth * Mathf.Tan(cam.fieldOfView * 0.5f * Mathf.Deg2Rad);
        float size = viewHeight * ScreenSize;

        transform.rotation = camTransform.rotation;
        transform.localScale = Vector3.one * size;

        // Pulled toward the camera by its radius so the surface the pin is on doesn't cut the circle in half
        float radius = transform.lossyScale.x * 0.5f;
        transform.position = pin.position + (camTransform.position - pin.position).normalized * radius;

        SetOccluded(IsOccluded(camTransform.position));
    }

    // Anything but pins between the camera and the circle. The circle sits off the surface toward the camera, so the surface itself never counts
    bool IsOccluded(Vector3 camPosition) {
        Vector3 toPin = transform.position - camPosition;
        int count = Physics.RaycastNonAlloc(camPosition, toPin.normalized, _hits, toPin.magnitude, OcclusionLayers, QueryTriggerInteraction.Ignore);
        for (int i = 0; i < count; i++) {
            if (_hits[i].collider.GetComponentInParent<PinObject>() == null) return true;
        }

        return false;
    }

    void SetOccluded(bool occluded) {
        if (occluded == _occluded) return;
        _occluded = occluded;

        foreach (SpriteRenderer renderer in _renderers) {
            renderer.color = occluded ? OccludedColor : _visibleColor;
        }
        
        if (_pin != null) _pin.SetOccluded(occluded);
    }
}
