using System.Collections;
using System.Runtime.InteropServices;
using UnityEngine;

public class ScreenshotManager: Singleton<ScreenshotManager> {

    [Header("Settings")]
    public Texture2D watermarkTexture;
    public LayerMask uiLayerMask;

    [DllImport("__Internal")]
    private static extern void DownloadImage(byte[] array, int byteLength, string fileName);

    public void DownloadScreenshot() {
        StartCoroutine(CaptureScreenshotRoutine());
    }

    private IEnumerator CaptureScreenshotRoutine() {
        // Wait for end of frame to ensure active rendering is complete
        yield return new WaitForEndOfFrame();

        Camera mainCamera = Camera.main;

        int width = Screen.width;
        int height = Screen.height;

        // 1. Temporarily remove UI layer from Main Camera Culling Mask
        int originalCullingMask = mainCamera.cullingMask;
        mainCamera.cullingMask &= ~uiLayerMask;

        // 2. Render camera view to a RenderTexture
        RenderTexture rt = new RenderTexture(width, height, 24);
        mainCamera.targetTexture = rt;
        RenderTexture.active = rt;
        mainCamera.Render();

        // 3. Read pixels into a Texture2D
        Texture2D screenShot = new Texture2D(width, height, TextureFormat.RGB24, false);
        screenShot.ReadPixels(new Rect(0, 0, width, height), 0, 0);

        // Restore camera settings immediately
        mainCamera.targetTexture = null;
        RenderTexture.active = null;
        mainCamera.cullingMask = originalCullingMask;
        Destroy(rt);

        // 4. Draw Watermark if assigned
        if (watermarkTexture != null) {
            ApplyWatermark(screenShot, watermarkTexture);
        }

        screenShot.Apply();

        // 5. Encode to PNG
        byte[] imageBytes = screenShot.EncodeToPNG();
        Destroy(screenShot);

        // 6. Download file in WebGL browser
#if UNITY_WEBGL && !UNITY_EDITOR
        DownloadImage(imageBytes, imageBytes.Length, "Screenshot.png");
#else
        System.IO.File.WriteAllBytes(Application.dataPath + "/Screenshot.png", imageBytes);
        Debug.Log($"Screenshot saved to {Application.dataPath + "/Screenshot.png"} (Editor Mode)");
#endif
    }

    private void ApplyWatermark(Texture2D background, Texture2D watermark) {
        // Example: Draw watermark in bottom-right corner
        int startX = background.width - watermark.width - 20; // 20px padding
        int startY = 20; // 20px padding from bottom

        for (int x = 0; x < watermark.width; x++) {
            for (int y = 0; y < watermark.height; y++) {
                Color watermarkPixel = watermark.GetPixel(x, y);
                if (watermarkPixel.a > 0) // Blend non-transparent pixels
                {
                    int targetX = startX + x;
                    int targetY = startY + y;

                    if (targetX >= 0 && targetX < background.width && targetY >= 0 && targetY < background.height) {
                        Color backgroundPixel = background.GetPixel(targetX, targetY);
                        Color blendedPixel = Color.Lerp(backgroundPixel, watermarkPixel, watermarkPixel.a);
                        background.SetPixel(targetX, targetY, blendedPixel);
                    }
                }
            }
        }
    }
}
