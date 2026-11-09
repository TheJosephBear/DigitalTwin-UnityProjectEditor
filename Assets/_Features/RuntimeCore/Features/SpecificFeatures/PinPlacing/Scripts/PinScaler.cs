using UnityEngine;

/// <summary>
/// Keeps the 3D pin model the same size on screen from any distance within ConstantSizeDistance.
/// Sits on the model, a child of the pin, so the pin's own transform stays at the clicked point.
/// </summary>
public class PinScaler : MonoBehaviour {

    [Tooltip("Height of the whole pin on screen, as a fraction of the screen height")]
    public float ScreenHeight = 0.134f;

    [Tooltip("Height of the model at scale 1 (leg tip to the top of the head)")]
    public float ModelHeight = 5.6f;

    [Tooltip("[min, max] camera distance in which the pin keeps its size on screen. Closer than min it stops shrinking (grows on screen), farther than max it stops growing (shrinks on screen)")]
    public Vector2 ConstantSizeDistance = new Vector2(2f, 300f);

    void OnEnable() {
        Camera.onPreCull += OnCameraPreCull;
    }

    void OnDisable() {
        Camera.onPreCull -= OnCameraPreCull;
    }

    // Right before rendering, after everything (RTG, Cinemachine) has moved the camera this frame
    void OnCameraPreCull(Camera cam) {
        if (cam != Camera.main) return;

        Transform camTransform = cam.transform;
        float viewHeight;
        if (cam.orthographic) {
            viewHeight = 2f * cam.orthographicSize;
        } else {
            // Perspective size on screen depends on the depth along the view direction, not the distance
            float depth = Vector3.Dot(transform.position - camTransform.position, camTransform.forward);
            depth = Mathf.Clamp(depth, ConstantSizeDistance.x, ConstantSizeDistance.y);
            viewHeight = 2f * depth * Mathf.Tan(cam.fieldOfView * 0.5f * Mathf.Deg2Rad);
        }

        transform.localScale = Vector3.one * (viewHeight * ScreenHeight / ModelHeight);
    }
}
