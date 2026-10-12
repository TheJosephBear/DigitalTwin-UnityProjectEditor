using UnityEngine;

public class ScreenshotButton: MonoBehaviour {
    public void OnPress() {
        ScreenshotManager.Instance.DownloadScreenshot();
    }
}
