using Cinemachine;
using UnityEngine;

/// <summary>
/// Renders the scene from a given view with the main camera, without moving the camera visibly.
/// The camera is placed at the view directly and rendered right away. Going through Cinemachine
/// (activating the vcam) moved the camera only on the next brain update, so the capture showed the previous view.
/// </summary>
public static class ViewRenderer {

    /// <summary>Renders the view point (its vcam position and lens). Returns null without a main camera.</summary>
    public static RenderTexture Render(ViewPoint viewPoint, int width, int height) {
        if (viewPoint == null) return null;

        Transform viewTransform = viewPoint.vcam != null ? viewPoint.vcam.transform : viewPoint.transform;
        var vcam = viewPoint.vcam != null ? viewPoint.vcam.GetComponent<CinemachineVirtualCamera>() : null;
        return Render(viewTransform.position, viewTransform.rotation, vcam != null ? vcam.m_Lens.FieldOfView : -1f, width, height);
    }

    /// <summary>Renders from the given pose, fieldOfView below 0 keeps the camera's own. Returns null without a main camera.</summary>
    public static RenderTexture Render(Vector3 position, Quaternion rotation, float fieldOfView, int width, int height) {
        Camera unityCamera = Camera.main;
        if (unityCamera == null) return null;

        Transform camTransform = unityCamera.transform;
        Vector3 originalPosition = camTransform.position;
        Quaternion originalRotation = camTransform.rotation;
        float originalFov = unityCamera.fieldOfView;

        if (fieldOfView > 0f) unityCamera.fieldOfView = fieldOfView;
        camTransform.SetPositionAndRotation(position, rotation);

        // Note: We use a Depth of 24 for a standard 3D render
        RenderTexture rt = new RenderTexture(width, height, 24);
        rt.Create();

        RenderTexture previousRT = unityCamera.targetTexture;
        unityCamera.targetTexture = rt;
        unityCamera.aspect = (float)width / height;
        unityCamera.Render();
        unityCamera.targetTexture = previousRT;
        unityCamera.ResetAspect();

        camTransform.SetPositionAndRotation(originalPosition, originalRotation);
        unityCamera.fieldOfView = originalFov;

        return rt;
    }

    /// <summary>Copies a rendered view into a readable texture (for encoding) and releases the render texture.</summary>
    public static Texture2D ToTexture2D(RenderTexture rt) {
        if (rt == null) return null;

        RenderTexture previousActive = RenderTexture.active;
        RenderTexture.active = rt;
        Texture2D texture = new Texture2D(rt.width, rt.height, TextureFormat.RGB24, false);
        texture.ReadPixels(new Rect(0, 0, rt.width, rt.height), 0, 0);
        texture.Apply();
        RenderTexture.active = previousActive;

        rt.Release();
        Object.Destroy(rt);
        return texture;
    }
}
