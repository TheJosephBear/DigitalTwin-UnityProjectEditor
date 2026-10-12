using System.Runtime.InteropServices;
using UnityEngine;

public class WebGLTabOpener: MonoBehaviour {
    // Singleton for simple calling
    private static WebGLTabOpener _instance;
    public bool IsDontDestroyOnLoad = false;

    public static WebGLTabOpener Instance {
        get {
            return _instance;
        }
    }

    protected virtual void Awake() {
        if (_instance == null) {
            _instance = this;
            if(IsDontDestroyOnLoad) DontDestroyOnLoad(gameObject);
        } else if (_instance != this) {
            Destroy(gameObject);
        }
    }


    // Import the native JavaScript function declared in the .jslib file
    [DllImport("__Internal")]
    private static extern void OpenNewTab(string url);

    /// <summary>
    /// Call this method (e.g. from a UI Button OnClick listener)
    /// </summary>
    public void OpenLinkInNewTab(string url) {
#if UNITY_WEBGL && !UNITY_EDITOR
        OpenNewTab(url);
#else
        // Fallback for running inside the Unity Editor or standalone builds
        Application.OpenURL(url);
#endif
    }
}
