using UnityEngine;
using RTG;

[DisallowMultipleComponent]
public class CameraCollisionEnforcer : MonoBehaviour {
    [Header("Collision Settings")]
    [Tooltip("Layer mask for geometry the camera should collide with.")]
    public LayerMask collisionLayers = -1;

    [Tooltip("Radius of the camera's sphere collision boundary.")]
    public float cameraRadius = 0.5f;

    [Tooltip("Max iterations to resolve penetration if the camera gets stuck inside geometry.")]
    public int maxDepenetrationSteps = 3;

    [Header("Optional Focus Raycasting")]
    [Tooltip("If true, prevents geometry from blocking the view between the camera and its focus point.")]
    public bool preventObstaclesInView = false;

    private Transform _cameraTransform;
    private Vector3 _previousValidPosition;
    private RTFocusCamera _camScriptRTG;

    private void Start() {
        _camScriptRTG = GetComponent<RTFocusCamera>();
        // Cache the target camera transform directly from the RTFocusCamera singleton
        if (_camScriptRTG != null && _camScriptRTG.TargetCamera != null) {
            _cameraTransform = _camScriptRTG.TargetCamera.transform;
            _previousValidPosition = _cameraTransform.position;
        }
    }

    private void LateUpdate() {
        if (_cameraTransform == null) {
            if (_camScriptRTG != null && _camScriptRTG.TargetCamera != null) {
                _cameraTransform = _camScriptRTG.TargetCamera.transform;
                _previousValidPosition = _cameraTransform.position;
            }
            return;
        }

        // Handle collision after RTFocusCamera updates in Update()
        ResolveCameraCollisions();
    }

    private void ResolveCameraCollisions() {
        Vector3 targetPos = _cameraTransform.position;

        // 1. Raycast check from previous frame's valid position to current target position
        Vector3 direction = targetPos - _previousValidPosition;
        float distance = direction.magnitude;

        if (distance > 0.001f) {
            if (Physics.SphereCast(_previousValidPosition, cameraRadius, direction.normalized, out RaycastHit hit, distance, collisionLayers)) {
                // Place camera right at the impact point minus the radius padding
                targetPos = _previousValidPosition + direction.normalized * (hit.distance - 0.01f);
            }
        }

        // 2. Overlap check & depenetration (if camera started or ended up inside a collider)
        Collider[] overlaps = Physics.OverlapSphere(targetPos, cameraRadius, collisionLayers);
        for (int i = 0; i < overlaps.Length && i < maxDepenetrationSteps; i++) {
            Collider col = overlaps[i];
            if (Physics.ComputePenetration(
                GetComponent<SphereCollider>() != null ? GetComponent<SphereCollider>() : null,
                targetPos,
                Quaternion.identity,
                col,
                col.transform.position,
                col.transform.rotation,
                out Vector3 directionToPush,
                out float distanceToPush)) {
                targetPos += directionToPush * (distanceToPush + 0.01f);
            }
        }

        // Re-assign the clamped, non-colliding position back to the camera
        _cameraTransform.position = targetPos;
        _previousValidPosition = targetPos;
    }

    private void OnEnable() {
        // Reset valid position to current location so SphereCast doesn't trace back to old pre-disabled data
        if (_cameraTransform != null) {
            _previousValidPosition = _cameraTransform.position;
        }
    }

    private void OnDrawGizmosSelected() {
        // Visualize the camera collision sphere in Scene view
        Gizmos.color = Color.yellow;
        Vector3 pos = _cameraTransform != null ? _cameraTransform.position : transform.position;
        Gizmos.DrawWireSphere(pos, cameraRadius);
    }
}
